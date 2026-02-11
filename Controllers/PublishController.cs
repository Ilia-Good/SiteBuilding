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

    public record PublishRequest(string? Slug, string? Html, BuilderStateRequest? State);

    public record BuilderStateRequest(
        string? SiteTitle,
        string? SiteTitleColor,
        string? HeaderText,
        string? HeaderAlign,
        string? HeaderColor,
        string? FooterText,
        string? FooterAlign,
        string? FooterColor,
        string? AvatarUrl,
        string? BackgroundColor,
        string? GlobalEffect,
        List<BuilderBlockRequest>? Blocks
    );

    public record BuilderBlockRequest(
        int? Id,
        string? Type,
        string? Content,
        string? Align,
        string? Color,
        string? Effect,
        string? Url,
        int? PaddingY,
        int? PaddingX,
        string? ButtonColor,
        string? SubmitLabel
    );

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
            else
            {
                var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
                var isUpdated = false;
                if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    isUpdated = true;
                }

                if (isUpdated)
                {
                    await _db.SaveChangesAsync();
                }
            }

            var builtFromState = request.State is not null;
            var formEndpoint = ResolveFormEndpoint(user);
            var hasContactForm = builtFromState && (request.State?.Blocks?.Any(b => NormalizeType(b.Type) == "contactForm") ?? false);
            if (hasContactForm && string.IsNullOrWhiteSpace(formEndpoint))
            {
                return BadRequest("Для формы обратной связи сначала укажите Formspree endpoint в настройках конструктора.");
            }

            var html = builtFromState
                ? BuildFinalHtml(request.State!, formEndpoint ?? string.Empty)
                : request.Html ?? string.Empty;
            if (string.IsNullOrWhiteSpace(html))
            {
                return BadRequest("Пустой HTML не может быть опубликован.");
            }

            if (html.Length > MaxHtmlLength)
            {
                return BadRequest("HTML слишком большой. Уменьшите контент и попробуйте снова.");
            }

            if (!builtFromState)
            {
                html = StripScriptTags(html);
            }

            if (GetTotalTextLength(html) > MaxTotalTextLength)
            {
                return BadRequest("Слишком много текста. Укоротите блоки и попробуйте снова.");
            }

            if (GetMaxImageUrlLength(html) > MaxImageUrlLength)
            {
                return BadRequest("Слишком длинный URL изображения. Укоротите ссылку.");
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
                else if (getResp.StatusCode != HttpStatusCode.NotFound)
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
            site.PublishedAt = now;
            site.IsActive = true;

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

    private static string? ResolveFormEndpoint(ApplicationUser user)
    {
        var endpoint = user.FormEndpoint?.Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return null;
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsed))
        {
            return null;
        }

        if (!string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!string.Equals(parsed.Host, "formspree.io", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!parsed.AbsolutePath.StartsWith("/f/", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return endpoint;
    }

    private static string BuildFinalHtml(BuilderStateRequest state, string formEndpoint)
    {
        var title = state.SiteTitle?.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            title = "My site";
        }

        var titleColor = SafeColor(state.SiteTitleColor, "#f8fafc")!;
        var headerText = state.HeaderText?.Trim() ?? string.Empty;
        var headerAlign = SafeAlign(state.HeaderAlign);
        var headerColor = SafeColor(state.HeaderColor, "#e5e7eb")!;
        var footerText = state.FooterText?.Trim() ?? string.Empty;
        var footerAlign = SafeAlign(state.FooterAlign);
        var footerColor = SafeColor(state.FooterColor, "#e5e7eb")!;
        var avatarUrl = state.AvatarUrl?.Trim() ?? string.Empty;
        var backgroundColor = SafeColor(state.BackgroundColor, "#0b1220")!;
        var globalEffectClass = ToEffectClass(state.GlobalEffect);

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"ru\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\" />");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"  <title>{EscapeHtml(title)}</title>");
        if (!string.IsNullOrWhiteSpace(avatarUrl))
        {
            html.AppendLine($"  <link rel=\"icon\" href=\"{EscapeHtml(avatarUrl)}\" />");
        }

        html.AppendLine("  <style>");
        html.AppendLine(BuildExportCss(backgroundColor));
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        html.AppendLine($"  <div class=\"site-title {globalEffectClass}\" style=\"text-align:{headerAlign};color:{EscapeHtml(titleColor)}\">{EscapeHtml(title)}</div>");

        if (!string.IsNullOrWhiteSpace(headerText) || !string.IsNullOrWhiteSpace(avatarUrl))
        {
            html.AppendLine($"  <header class=\"site-header {globalEffectClass}\" style=\"text-align:{headerAlign};color:{EscapeHtml(headerColor)}\">");
            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                html.AppendLine($"    <img class=\"site-avatar\" src=\"{EscapeHtml(avatarUrl)}\" alt=\"Logo\" />");
            }

            if (!string.IsNullOrWhiteSpace(headerText))
            {
                html.AppendLine($"    <div>{EscapeHtml(headerText)}</div>");
            }

            html.AppendLine("  </header>");
        }

        html.AppendLine("  <main>");
        foreach (var block in state.Blocks ?? [])
        {
            AppendBlockHtml(html, block, formEndpoint);
        }
        html.AppendLine("  </main>");

        if (!string.IsNullOrWhiteSpace(footerText))
        {
            html.AppendLine($"  <footer class=\"{globalEffectClass}\" style=\"text-align:{footerAlign};color:{EscapeHtml(footerColor)}\"><div>{EscapeHtml(footerText)}</div></footer>");
        }

        html.AppendLine("  <script>");
        html.AppendLine("    document.querySelectorAll('.sb-contact-form').forEach(function(form) {");
        html.AppendLine("      form.addEventListener('submit', async function(event) {");
        html.AppendLine("        event.preventDefault();");
        html.AppendLine("        var card = form.closest('.contact-form-shell');");
        html.AppendLine("        var success = card ? card.querySelector('.sb-contact-success') : null;");
        html.AppendLine("        var error = card ? card.querySelector('.sb-contact-error') : null;");
        html.AppendLine("        if (success) success.hidden = true;");
        html.AppendLine("        if (error) error.hidden = true;");
        html.AppendLine("        try {");
        html.AppendLine("          var resp = await fetch(form.action, {");
        html.AppendLine("            method: 'POST',");
        html.AppendLine("            body: new FormData(form),");
        html.AppendLine("            headers: { 'Accept': 'application/json' }");
        html.AppendLine("          });");
        html.AppendLine("          if (!resp.ok) throw new Error('send_failed');");
        html.AppendLine("          form.reset();");
        html.AppendLine("          if (success) success.hidden = false;");
        html.AppendLine("        } catch (e) {");
        html.AppendLine("          if (error) error.hidden = false;");
        html.AppendLine("        }");
        html.AppendLine("      });");
        html.AppendLine("    });");
        html.AppendLine("  </script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private static void AppendBlockHtml(StringBuilder html, BuilderBlockRequest block, string formEndpoint)
    {
        var type = NormalizeType(block.Type);
        if (string.IsNullOrWhiteSpace(type))
        {
            return;
        }

        var align = SafeAlign(block.Align);
        var effectClass = ToEffectClass(block.Effect);
        var color = SafeColor(block.Color, null);
        var colorStyle = string.IsNullOrWhiteSpace(color) ? string.Empty : $"color:{EscapeHtml(color)};";
        var style = $"text-align:{align};{colorStyle}";

        switch (type)
        {
            case "section":
                html.AppendLine($"    <div class=\"{effectClass}\" style=\"{style}\"><section><h3>{EscapeHtml(block.Content ?? "Section")}</h3></section></div>");
                break;
            case "heading":
                html.AppendLine($"    <div class=\"{effectClass}\" style=\"{style}\"><h2>{EscapeHtml(block.Content ?? "Heading")}</h2></div>");
                break;
            case "text":
                html.AppendLine($"    <div class=\"{effectClass}\" style=\"{style}\"><p>{EscapeHtml(block.Content ?? "Text")}</p></div>");
                break;
            case "image":
                html.AppendLine($"    <div class=\"{effectClass}\" style=\"text-align:{align};\"><img src=\"{EscapeHtml(block.Content ?? string.Empty)}\" alt=\"\"></div>");
                break;
            case "button":
            {
                var label = EscapeHtml(block.Content ?? "Button");
                var url = EscapeHtml(block.Url ?? "#");
                var py = Math.Clamp(block.PaddingY ?? 8, 4, 40);
                var px = Math.Clamp(block.PaddingX ?? 16, 8, 80);
                var buttonColor = SafeColor(block.ButtonColor, "#2563eb")!;
                html.AppendLine($"    <div style=\"text-align:{align};\"><a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"button-link {effectClass}\" style=\"padding:{py}px {px}px;background:{EscapeHtml(buttonColor)};color:#fff\">{label}</a></div>");
                break;
            }
            case "contactForm":
            {
                var submitLabel = EscapeHtml(block.SubmitLabel ?? "Отправить");
                html.AppendLine($"    <div class=\"contact-form-shell {effectClass}\" style=\"text-align:{align};\">");
                html.AppendLine($"      <form class=\"sb-contact-form\" action=\"{EscapeHtml(formEndpoint)}\" method=\"POST\">");
                html.AppendLine("        <input type=\"text\" name=\"name\" placeholder=\"Ваше имя\" required>");
                html.AppendLine("        <input type=\"email\" name=\"email\" placeholder=\"Ваш email\" required>");
                html.AppendLine("        <textarea name=\"message\" placeholder=\"Сообщение\" rows=\"5\" required></textarea>");
                html.AppendLine($"        <button type=\"submit\">{submitLabel}</button>");
                html.AppendLine("      </form>");
                html.AppendLine("      <div class=\"sb-contact-success\" hidden>Сообщение отправлено</div>");
                html.AppendLine("      <div class=\"sb-contact-error\" hidden>Не удалось отправить сообщение. Попробуйте позже.</div>");
                html.AppendLine("    </div>");
                break;
            }
        }
    }

    private static string NormalizeType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var type = value.Trim();
        if (type.Equals("formspree", StringComparison.OrdinalIgnoreCase))
        {
            return "contactForm";
        }

        return type;
    }

    private static string SafeAlign(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "center" => "center",
            "right" => "right",
            _ => "left"
        };
    }

    private static string ToEffectClass(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "shadow" => "effect-shadow",
            "glow" => "effect-glow",
            _ => string.Empty
        };
    }

    private static string? SafeColor(string? value, string? fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var color = value.Trim();
        if (Regex.IsMatch(color, "^#[0-9a-fA-F]{6}$"))
        {
            return color;
        }

        return fallback;
    }

    private static string EscapeHtml(string text)
    {
        return WebUtility.HtmlEncode(text ?? string.Empty);
    }

    private static string BuildExportCss(string backgroundColor)
    {
        return $@"html, body {{ height: 100%; }}
body {{
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  margin: 0;
  padding: 1.5rem;
  background: {backgroundColor};
  color: #e5e7eb;
  font-family: system-ui, -apple-system, sans-serif;
}}
header, footer {{ margin-bottom: 1rem; }}
footer {{ margin-top: auto; }}
main {{
  flex: 1 0 auto;
  background: rgba(15, 23, 42, 0.9);
  padding: 1.25rem;
  border-radius: .75rem;
  box-shadow: 0 20px 50px rgba(15, 23, 42, 0.7);
}}
img {{ max-width: 100%; height: auto; display: block; margin: .75rem 0; border-radius: .5rem; }}
a.effect-shadow, .effect-shadow {{ box-shadow: 0 12px 30px rgba(0, 0, 0, 0.45); }}
a.effect-glow, .effect-glow {{ box-shadow: 0 0 24px rgba(99, 102, 241, 0.65); }}
.site-title {{ font-size: 1.5rem; font-weight: 700; margin-bottom: .75rem; }}
.site-header {{ display: flex; align-items: center; gap: .5rem; padding: .75rem 1rem; border-radius: .25rem; background: rgba(15, 23, 42, 0.9); }}
.site-avatar {{ width: 72px; height: 72px; object-fit: cover; border-radius: 12px; border: 2px solid rgba(255,255,255,.4); box-shadow: 0 8px 20px rgba(0,0,0,.45); }}
a.button-link {{
  display: inline-block;
  border-radius: 999px;
  border: none;
  background: #2563eb;
  color: #fff;
  text-decoration: none;
  cursor: pointer;
  box-shadow: 0 10px 30px rgba(99, 102, 241, 0.6);
}}
.contact-form-shell {{
  max-width: 640px;
  margin: 1rem auto;
  padding: 1rem;
  border-radius: 18px;
  border: 1px solid rgba(148, 163, 184, 0.35);
  background: linear-gradient(165deg, rgba(30, 41, 59, 0.95), rgba(15, 23, 42, 0.92));
}}
.sb-contact-form {{
  display: grid;
  gap: .7rem;
}}
.sb-contact-form input,
.sb-contact-form textarea {{
  width: 100%;
  box-sizing: border-box;
  border: 1px solid rgba(148, 163, 184, .4);
  border-radius: 12px;
  padding: .65rem .8rem;
  background: rgba(15, 23, 42, .8);
  color: #e5e7eb;
}}
.sb-contact-form button {{
  border: none;
  border-radius: 12px;
  padding: .7rem 1rem;
  font-weight: 600;
  background: linear-gradient(135deg, #2563eb, #22c55e);
  color: #fff;
  cursor: pointer;
}}
.sb-contact-success,
.sb-contact-error {{
  margin-top: .55rem;
  font-size: .95rem;
}}
.sb-contact-success {{ color: #34d399; }}
.sb-contact-error {{ color: #fca5a5; }}";
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
            if (match.Groups.Count < 2)
            {
                continue;
            }

            var length = match.Groups[1].Value.Length;
            if (length > max)
            {
                max = length;
            }
        }

        return max;
    }

    private static string EnsureTrailingSlash(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.EndsWith("/") ? value : value + "/";
    }

    private static DateTime GetUtcDate(DateTime value)
    {
        return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
    }
}

