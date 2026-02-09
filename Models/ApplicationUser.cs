using System.ComponentModel.DataAnnotations;

namespace SiteBuilder.Models;

public class ApplicationUser
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string GoogleId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<Site> Sites { get; set; } = new List<Site>();
}
