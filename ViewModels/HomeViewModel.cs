namespace SiteBuilder.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<PublicSiteCard> FeaturedSites { get; set; } = Array.Empty<PublicSiteCard>();
}
