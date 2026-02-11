using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiteBuilder.Controllers;

public class AccountController : Controller
{
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

    [HttpGet("account/login-failed")]
    public IActionResult LoginFailed()
    {
        ViewData["Title"] = "Ошибка входа";
        return View();
    }
}
