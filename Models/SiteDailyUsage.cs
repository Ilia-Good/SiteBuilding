using System.ComponentModel.DataAnnotations;

namespace SiteBuilder.Models;

public class SiteDailyUsage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SiteId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int EditsCount { get; set; }

    public Site? Site { get; set; }
}
