using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SiteBuilder.Controllers;

[Authorize]
public class BuilderController : Controller
{
    [HttpGet("builder")]
    public IActionResult Index()
    {
        ViewData["Title"] = "Page Builder";
        return View();
    }

    [HttpGet("builder/trial-expired")]
    public IActionResult TrialExpired()
    {
        ViewData["Title"] = "Пробный период";
        return View();
    }
}
