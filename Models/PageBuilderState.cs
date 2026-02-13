using System.Text.Json;

namespace SiteBuilder.Models;

public class PageBuilderState
{
    public List<PageBlock> Blocks { get; set; } = new();

    public static PageBuilderState Empty => new();

    public static PageBuilderState FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Empty;
        }

        try
        {
            var state = JsonSerializer.Deserialize<PageBuilderState>(json);
            if (state is null)
            {
                return Empty;
            }

            state.Blocks ??= new List<PageBlock>();
            return state;
        }
        catch
        {
            return Empty;
        }
    }
}
