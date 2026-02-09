using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Models;

namespace SiteBuilder.Controllers;

[Authorize]
[ApiController]
[Route("api/publish")]
public class PublishController : ControllerBase
{
    private const int MaxHtmlLength = 1_000_000;
    private const int MaxTotalTextLength = 5_000;
    private const int MaxImageUrlLength = 500;
    private const int MaxSiteNameLength = 50;
    private const int TrialHours = 48;
    private const int MaxSitesPerUser = 3;
    private const int MaxEditsPerDayPerSite = 3;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<PublishController> _logger;

    public PublishController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ApplicationDbContext db, ILogger<PublishController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _db = db;
        _logger = logger;
    }

    public record PublishRequest(string? Slug, string? Html);

    [HttpPost]
    public async Task<IActionResult> Publish([FromBody] PublishRequest request)
    {
        try
        {
            var slug = (request.Slug ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(slug))
            {
                return BadRequest("Укажите имя папки сайта.");
            }

        if (slug.Length > MaxSiteNameLength)
        {
            return BadRequest($"Имя сайта слишком длинное. Максимум {MaxSiteNameLength} символов.");
        }

        if (!IsValidSlug(slug))
        {
            return BadRequest("Имя сайта может содержать только латинские буквы, цифры и дефис.");
        }

        var html = request.Html ?? string.Empty;
        if (string.IsNullOrWhiteSpace(html))
        {
            return BadRequest("Пустой HTML не может быть опубликован.");
        }

        if (html.Length > MaxHtmlLength)
        {
            return BadRequest("HTML слишком большой. Уменьшите контент и попробуйте снова.");
        }

        html = StripScriptTags(html);

        if (GetTotalTextLength(html) > MaxTotalTextLength)
        {
            return BadRequest("Слишком много текста. Укоротите блоки и попробуйте снова.");
        }

        if (GetMaxImageUrlLength(html) > MaxImageUrlLength)
        {
            return BadRequest("Слишком длинный URL изображения. Укоротите ссылку.");
        }

        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            if (string.IsNullOrWhiteSpace(email))
            {
                return Unauthorized();
            }

            user = new ApplicationUser
            {
                Id = userId,
                Email = email,
                GoogleId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString("N"),
                CreatedAt = now
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        var existingWithName = await _db.Sites.FirstOrDefaultAsync(s => s.SiteName == slug);
        if (existingWithName is not null && existingWithName.OwnerUserId != userId)
        {
            return BadRequest("Имя сайта уже занято. Выберите другое.");
        }

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.SiteName == slug);
        if (site is null)
        {
            var totalCount = await _db.Sites.CountAsync(s => s.OwnerUserId == userId);
            if (totalCount >= MaxSitesPerUser)
            {
                return BadRequest($"Лимит сайтов: {MaxSitesPerUser}. Удалите один из своих сайтов, чтобы создать новый.");
            }

            site = new Site
            {
                Id = Guid.NewGuid(),
                OwnerUserId = userId,
                SiteName = slug,
                CreatedAt = now,
                ExpiresAt = now.AddHours(TrialHours),
                IsPaid = false,
                GithubPath = $"sites/{slug}/index.html"
            };

            _db.Sites.Add(site);
            await _db.SaveChangesAsync();
        }
        else if (!site.IsPaid && site.ExpiresAt <= now)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Пробный период истёк. Отметьте сайт как оплаченный.");
        }

        var today = GetUtcDate(now);
        var usage = await _db.SiteDailyUsages.FirstOrDefaultAsync(u => u.SiteId == site.Id && u.Date == today);
        if (usage is not null && usage.EditsCount >= MaxEditsPerDayPerSite)
        {
            return BadRequest($"Лимит изменений на сегодня: {MaxEditsPerDayPerSite}.");
        }

        var owner = _configuration["GitHubPublish:Owner"] ?? "Ilia-Good";
        var repo = _configuration["GitHubPublish:Repo"] ?? "Goods";
        var branch = _configuration["GitHubPublish:Branch"] ?? "main";
        var pagesBase = _configuration["GitHubPublish:PagesBaseUrl"] ?? $"https://{owner.ToLowerInvariant()}.github.io/{repo}/";
        var token = _configuration["GITHUB_TOKEN"] ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");

        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(500, "Токен публикации не настроен на сервере.");
        }

        var filePath = $"sites/{slug}/index.html";
        var apiBase = $"https://api.github.com/repos/{owner}/{repo}/contents/{filePath}";
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MiniSiteBuilder", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        string? sha = null;
        using (var getResp = await client.GetAsync($"{apiBase}?ref={branch}"))
        {
            if (getResp.IsSuccessStatusCode)
            {
                var json = await getResp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("sha", out var shaEl))
                {
                    sha = shaEl.GetString();
                }
            }
            else if (getResp.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var err = await getResp.Content.ReadAsStringAsync();
                return StatusCode((int)getResp.StatusCode, err);
            }
        }

        var payload = new
        {
            message = $"Publish site: {slug}",
            content = Convert.ToBase64String(Encoding.UTF8.GetBytes(html)),
            branch,
            sha
        };

        var putJson = JsonSerializer.Serialize(payload);
        using var putResp = await client.PutAsync(apiBase, new StringContent(putJson, Encoding.UTF8, "application/json"));
        if (!putResp.IsSuccessStatusCode)
        {
            var err = await putResp.Content.ReadAsStringAsync();
            return StatusCode((int)putResp.StatusCode, err);
        }

        site.GithubPath = filePath;

        if (usage is null)
        {
            usage = new SiteDailyUsage
            {
                Id = Guid.NewGuid(),
                SiteId = site.Id,
                Date = today,
                EditsCount = 1
            };
            _db.SiteDailyUsages.Add(usage);
        }
        else
        {
            usage.EditsCount += 1;
        }

        await _db.SaveChangesAsync();

            var url = EnsureTrailingSlash(pagesBase) + $"sites/{slug}/";
            return Ok(new { url });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Publish failed");
            return StatusCode(StatusCodes.Status500InternalServerError, "Ошибка публикации. Попробуйте позже.");
        }
    }

    private static bool IsValidSlug(string slug)
    {
        return Regex.IsMatch(slug, "^[a-z0-9-]+$");
    }

    private static string StripScriptTags(string html)
    {
        return Regex.Replace(html, "(?is)<script.*?>.*?</script>", string.Empty);
    }

    private static int GetTotalTextLength(string html)
    {
        var text = Regex.Replace(html, "<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        return text.Trim().Length;
    }

    private static int GetMaxImageUrlLength(string html)
    {
        var matches = Regex.Matches(html, "<img[^>]+src\\s*=\\s*\"([^\"]*)\"", RegexOptions.IgnoreCase);
        var max = 0;
        foreach (Match match in matches)
        {
            if (match.Groups.Count < 2) continue;
            var length = match.Groups[1].Value.Length;
            if (length > max) max = length;
        }
        return max;
    }

    private static string EnsureTrailingSlash(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.EndsWith("/") ? value : value + "/";
    }

    private static DateTime GetUtcDate(DateTime value)
    {
        return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}
