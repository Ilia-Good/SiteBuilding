using SiteBuilder.Models;

namespace SiteBuilder.ViewModels;

public class SitesIndexViewModel
{
    public IReadOnlyList<Site> Sites { get; set; } = Array.Empty<Site>();
    public int MaxSites { get; set; }
    public bool CanCreate { get; set; }
    public string LimitMessage { get; set; } = string.Empty;
    public int UsedSitesCount { get; set; }
    public int RemainingSitesCount { get; set; }
    public int MaxEditsPerDayPerSite { get; set; } = 3;
    public IReadOnlyDictionary<Guid, int> UsedEditsTodayBySiteId { get; set; } = new Dictionary<Guid, int>();
    public IReadOnlyDictionary<Guid, int> RemainingEditsTodayBySiteId { get; set; } = new Dictionary<Guid, int>();
}
