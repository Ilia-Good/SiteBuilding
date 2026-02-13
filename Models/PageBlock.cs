using System.Text.Json;

namespace SiteBuilder.Models;

public class PageBlock
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public Dictionary<string, JsonElement> Props { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
