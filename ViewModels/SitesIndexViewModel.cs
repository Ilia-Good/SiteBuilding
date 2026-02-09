using SiteBuilder.Models;

namespace SiteBuilder.ViewModels;

public class SitesIndexViewModel
{
    public IReadOnlyList<Site> Sites { get; set; } = Array.Empty<Site>();
    public int MaxSites { get; set; }
    public bool CanCreate { get; set; }
    public string LimitMessage { get; set; } = string.Empty;
}
