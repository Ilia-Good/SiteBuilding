using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.ViewModels;

namespace SiteBuilder.Controllers;

public class PublicSitesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public PublicSitesController(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    [HttpGet("explore")]
    public async Task<IActionResult> Index()
    {
        var pagesBase = _configuration["GitHubPublish:PagesBaseUrl"] ?? string.Empty;
        pagesBase = EnsureTrailingSlash(pagesBase);

        var sites = await _db.Sites
            .Where(s => !string.IsNullOrWhiteSpace(s.GithubPath) && s.IsActive)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new PublicSiteCard
            {
                SiteName = s.SiteName,
                AuthorEmail = s.Owner != null ? s.Owner.Email : "",
                Url = pagesBase + "sites/" + s.SiteName + "/",
                CreatedAt = s.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();

        ViewData["Title"] = "Мини-сайты";
        return View(sites);
    }

    private static string EnsureTrailingSlash(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.EndsWith("/") ? value : value + "/";
    }
}
