using System.ComponentModel.DataAnnotations;

namespace SiteBuilder.Models;

public class ContactMessage
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid SiteId { get; set; }

    [Required]
    public Guid SiteOwnerUserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string MessageText { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string SenderIp { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsSpam { get; set; }

    public Site? Site { get; set; }
}
