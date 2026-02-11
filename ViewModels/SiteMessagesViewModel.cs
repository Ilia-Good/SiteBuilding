using SiteBuilder.Models;

namespace SiteBuilder.ViewModels;

public class SiteMessagesViewModel
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public IReadOnlyList<ContactMessage> Messages { get; set; } = Array.Empty<ContactMessage>();
    public string ErrorMessage { get; set; } = string.Empty;
}
