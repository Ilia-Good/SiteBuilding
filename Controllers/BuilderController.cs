using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SiteBuilder.Controllers;

[Authorize]
public class BuilderController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly HtmlTemplateGenerator _templateGenerator;

    public BuilderController(ApplicationDbContext db, HtmlTemplateGenerator templateGenerator)
    {
        _db = db;
        _templateGenerator = templateGenerator;
    }

    [HttpGet("builder")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Page Builder";
        return View();
    }

    [HttpGet("builder/template")]
    public IActionResult TemplateGenerator()
    {
        ViewData["Title"] = "Генератор HTML шаблонов";
        return View();
    }

    [HttpGet("builder/trial-expired")]
    public IActionResult TrialExpired()
    {
        ViewData["Title"] = "Пробный период истёк";
        return View();
    }

    [HttpGet("api/builder/state/{slug}")]
    public async Task<IActionResult> GetSiteState(string slug)
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var normalizedSlug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalizedSlug))
        {
            return BadRequest(new { error = "slug_required" });
        }

        var site = await _db.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.OwnerUserId == userId && s.SiteName == normalizedSlug);
        if (site is null)
        {
            return NotFound(new { error = "site_not_found" });
        }

        if (string.IsNullOrWhiteSpace(site.BuilderStateJson))
        {
            return NotFound(new { error = "state_not_found" });
        }

        try
        {
            using var doc = JsonDocument.Parse(site.BuilderStateJson);
            return Ok(doc.RootElement.Clone());
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "state_invalid" });
        }
    }

    /// <summary>
    /// Генерирует HTML шаблон мини-сайта с формой обратной связи через Formspree
    /// </summary>
    [HttpPost("builder/generate-template")]
    public IActionResult GenerateTemplate([FromBody] TemplateRequest request)
    {
        try
        {
            // Получаем email из Google аккаунта пользователя
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
                userEmail = User.FindFirst("preferred_email")?.Value;

            if (string.IsNullOrEmpty(userEmail))
                return BadRequest(new { error = "Email пользователя не найден" });

            // Генерируем HTML
            var htmlTemplate = _templateGenerator.GenerateTemplate(
                userEmail,
                siteName: request?.SiteName ?? "Мой сайт",
                siteDescription: request?.SiteDescription ?? "Добро пожаловать на мой сайт"
            );

            return Ok(new { html = htmlTemplate });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Скачивает сгенерированный HTML как файл
    /// </summary>
    [HttpPost("builder/download-template")]
    public IActionResult DownloadTemplate([FromBody] TemplateRequest request)
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            
            if (string.IsNullOrEmpty(userEmail))
                userEmail = User.FindFirst("preferred_email")?.Value;

            if (string.IsNullOrEmpty(userEmail))
                return BadRequest(new { error = "Email пользователя не найден" });

            var htmlTemplate = _templateGenerator.GenerateTemplate(
                userEmail,
                siteName: request?.SiteName ?? "Мой сайт",
                siteDescription: request?.SiteDescription ?? "Добро пожаловать на мой сайт"
            );

            var bytes = System.Text.Encoding.UTF8.GetBytes(htmlTemplate);
            return File(bytes, "text/html", $"{request?.SiteName ?? "site"}.html");
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

/// <summary>
/// Модель для запроса на создание шаблона
/// </summary>
public class TemplateRequest
{
    public string? SiteName { get; set; }
    public string? SiteDescription { get; set; }
}
