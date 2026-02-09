using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.ViewModels;

namespace SiteBuilder.Controllers;

[Authorize]
public class SitesController : Controller
{
    private const int MaxSitesPerUser = 3;
    private readonly ApplicationDbContext _db;

    public SitesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("sites")]
    public async Task<IActionResult> Index()
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var sites = await _db.Sites
            .Where(s => s.OwnerUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        ViewData["Title"] = "Мои сайты";
        var canCreate = sites.Count < MaxSitesPerUser;
        var model = new SitesIndexViewModel
        {
            Sites = sites,
            MaxSites = MaxSitesPerUser,
            CanCreate = canCreate,
            LimitMessage = canCreate
                ? string.Empty
                : $"У вас уже есть {MaxSitesPerUser} сайта(ов). Измените существующий или удалите один."
        };

        return View(model);
    }

    [HttpPost("sites/delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == id && s.OwnerUserId == userId);
        if (site is null)
        {
            return NotFound();
        }

        var usages = _db.SiteDailyUsages.Where(u => u.SiteId == site.Id);
        _db.SiteDailyUsages.RemoveRange(usages);
        _db.Sites.Remove(site);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}
