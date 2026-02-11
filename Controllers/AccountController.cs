using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using System.Security.Claims;

namespace SiteBuilder.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("account/login")]
    public IActionResult Login(string? returnUrl = "/builder")
    {
        var props = new AuthenticationProperties { RedirectUri = returnUrl ?? "/builder" };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpPost("account/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet("api/account/form-endpoint")]
    public async Task<IActionResult> GetFormEndpoint()
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            email = user.Email,
            formEndpoint = user.FormEndpoint ?? string.Empty
        });
    }

    public record FormEndpointRequest(string? FormEndpoint);

    [Authorize]
    [HttpPost("api/account/form-endpoint")]
    public async Task<IActionResult> SaveFormEndpoint([FromBody] FormEndpointRequest request)
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return NotFound();
        }

        var endpoint = (request.FormEndpoint ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return BadRequest("Укажите endpoint Formspree.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsed) ||
            !string.Equals(parsed.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(parsed.Host, "formspree.io", StringComparison.OrdinalIgnoreCase) ||
            !parsed.AbsolutePath.StartsWith("/f/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Некорректный endpoint. Нужен формат: https://formspree.io/f/XXXXXX");
        }

        user.FormEndpoint = endpoint;
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }
}
