using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;

namespace SiteBuilder.ViewComponents;

public class UserSitesMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public UserSitesMenuViewComponent(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return View(Array.Empty<SiteBuilder.Models.Site>());
        }

        var userIdRaw = HttpContext.User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return View(Array.Empty<SiteBuilder.Models.Site>());
        }

        var sites = await _db.Sites
            .Where(s => s.OwnerUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .AsNoTracking()
            .ToListAsync();

        return View(sites);
    }
}
