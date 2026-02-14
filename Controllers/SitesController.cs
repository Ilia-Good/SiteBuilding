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
    private const int MaxEditsPerDayPerSite = 3;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SitesController> _logger;

    public SitesController(ApplicationDbContext db, ILogger<SitesController> logger)
    {
        _db = db;
        _logger = logger;
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

        var siteIds = sites.Select(s => s.Id).ToList();
        var today = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0, DateTimeKind.Utc);
        var usageToday = siteIds.Count == 0
            ? new List<SiteDailyUsage>()
            : await _db.SiteDailyUsages
                .AsNoTracking()
                .Where(u => siteIds.Contains(u.SiteId) && u.Date == today)
                .ToListAsync();

        var usedEdits = usageToday
            .GroupBy(u => u.SiteId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.EditsCount));
        var remainingEdits = siteIds.ToDictionary(
            id => id,
            id => Math.Max(0, MaxEditsPerDayPerSite - (usedEdits.TryGetValue(id, out var used) ? used : 0))
        );

        ViewData["Title"] = "Мои сайты";
        var canCreate = sites.Count < MaxSitesPerUser;
        var model = new SitesIndexViewModel
        {
            Sites = sites,
            MaxSites = MaxSitesPerUser,
            CanCreate = canCreate,
            UsedSitesCount = sites.Count,
            RemainingSitesCount = Math.Max(0, MaxSitesPerUser - sites.Count),
            MaxEditsPerDayPerSite = MaxEditsPerDayPerSite,
            UsedEditsTodayBySiteId = usedEdits,
            RemainingEditsTodayBySiteId = remainingEdits,
            LimitMessage = canCreate
                ? string.Empty
                : $"У вас уже есть {MaxSitesPerUser} сайта(ов). Измените существующий или удалите один."
        };

        return View(model);
    }

    [HttpPost("sites/delete/{id:guid}")]
    [ValidateAntiForgeryToken]
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

        try
        {
            // Dependent rows are removed by cascade FK rules.
            _db.Sites.Remove(site);
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete site {SiteId} for user {UserId}", id, userId);
            TempData["SitesError"] = "Не удалось удалить сайт. Попробуйте позже.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet("sites/messages/{siteId:guid}")]
    [HttpGet("Sites/{siteId:guid}/Messages")]
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

        ViewData["Title"] = $"Сообщения: {site.SiteName}";
        var model = new SiteMessagesViewModel
        {
            SiteId = site.Id,
            SiteName = site.SiteName,
            Messages = Array.Empty<ContactMessage>()
        };

        try
        {
            model.Messages = await _db.ContactMessages
                .Where(m => m.SiteId == siteId && !m.IsSpam)
                .OrderByDescending(m => m.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load contact messages for site {SiteId}", siteId);
            model.ErrorMessage = "Не удалось загрузить сообщения. Обновите страницу позже.";
        }

        return View(model);
    }
}
