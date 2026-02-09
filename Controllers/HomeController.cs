using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Models;
using SiteBuilder.ViewModels;

namespace SiteBuilder.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IConfiguration configuration)
    {
        _logger = logger;
        _db = db;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel
        {
            FeaturedSites = await LoadPublicSites(take: 6)
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task<IReadOnlyList<PublicSiteCard>> LoadPublicSites(int take)
    {
        var pagesBase = _configuration["GitHubPublish:PagesBaseUrl"] ?? string.Empty;
        pagesBase = EnsureTrailingSlash(pagesBase);

        var sites = await _db.Sites
            .Where(s => !string.IsNullOrWhiteSpace(s.GithubPath))
            .OrderByDescending(s => s.CreatedAt)
            .Take(take)
            .Select(s => new PublicSiteCard
            {
                SiteName = s.SiteName,
                AuthorEmail = s.Owner != null ? s.Owner.Email : "",
                Url = pagesBase + "sites/" + s.SiteName + "/",
                CreatedAt = s.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();

        return sites;
    }

    private static string EnsureTrailingSlash(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;
        return value.EndsWith("/") ? value : value + "/";
    }
}
