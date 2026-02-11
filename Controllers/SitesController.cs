using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Models;
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
        var messages = _db.ContactMessages.Where(m => m.SiteId == site.Id);
        _db.ContactMessages.RemoveRange(messages);
        _db.Sites.Remove(site);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet("sites/messages/{siteId:guid}")]
    public async Task<IActionResult> Messages(Guid siteId)
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var site = await _db.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == siteId && s.OwnerUserId == userId);
        if (site is null)
        {
            return NotFound();
        }

        var messages = await _db.ContactMessages
            .Where(m => m.SiteId == siteId && !m.IsSpam)
            .OrderByDescending(m => m.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        ViewData["Title"] = $"Сообщения: {site.SiteName}";
        var model = new SiteMessagesViewModel
        {
            SiteId = site.Id,
            SiteName = site.SiteName,
            Messages = messages
        };

        return View(model);
    }
}
