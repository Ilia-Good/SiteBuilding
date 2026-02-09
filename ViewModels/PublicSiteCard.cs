namespace SiteBuilder.ViewModels;

public class PublicSiteCard
{
    public string SiteName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
