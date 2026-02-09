using System.ComponentModel.DataAnnotations;

namespace SiteBuilder.Models;

public class Site
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerUserId { get; set; }

    [Required]
    [MaxLength(80)]
    public string SiteName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public bool IsPaid { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(256)]
    public string? GithubPath { get; set; }

    public ApplicationUser? Owner { get; set; }

    public ICollection<SiteDailyUsage> DailyUsages { get; set; } = new List<SiteDailyUsage>();
}
