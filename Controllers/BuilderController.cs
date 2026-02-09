using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteBuilder.Services;
using System.Security.Claims;

namespace SiteBuilder.Controllers;

[Authorize]
public class BuilderController : Controller
{
    private readonly HtmlTemplateGenerator _templateGenerator;

    public BuilderController(HtmlTemplateGenerator templateGenerator)
    {
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
