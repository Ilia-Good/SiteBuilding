using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Models;

namespace SiteBuilder.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private const int MinMessageLength = 5;
    private const int MaxMessageLength = 2000;
    private const int IpCooldownMinutes = 2;
    private const int MaxIpMessagesPerDay = 20;
    private const int MaxSiteMessagesPerDayFree = 50;

    private readonly ApplicationDbContext _db;

    public ContactController(ApplicationDbContext db)
    {
        _db = db;
    }

    public record SendContactRequest(Guid SiteId, string? Name, string? Email, string? Message, string? WebsiteField);
    public record SendContactFormRequest(string? SiteId, string? Name, string? Email, string? Message, string? WebsiteField);

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendContactRequest request)
    {
        return await HandleSend(request.SiteId, request.Name, request.Email, request.Message, request.WebsiteField);
    }

    [HttpPost("send-simple")]
    public async Task<IActionResult> SendSimple([FromForm] SendContactFormRequest request)
    {
        if (!Guid.TryParse(request.SiteId, out var siteId))
        {
            return BadRequest(new { ok = false, reason = "site_not_found" });
        }

        return await HandleSend(siteId, request.Name, request.Email, request.Message, request.WebsiteField);
    }

    private async Task<IActionResult> HandleSend(Guid siteId, string? rawName, string? rawEmail, string? rawMessage, string? websiteField)
    {
        var site = await _db.Sites
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == siteId);
        if (site is null || !site.IsActive || site.PublishedAt is null)
        {
            return NotFound(new { ok = false, reason = "site_not_found" });
        }

        var now = DateTime.UtcNow;
        var ip = GetSenderIp();
        var todayUtc = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);

        if (!string.IsNullOrWhiteSpace(websiteField))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { ok = false, reason = "spam" });
        }

        var name = (rawName ?? string.Empty).Trim();
        var email = (rawEmail ?? string.Empty).Trim();
        var message = (rawMessage ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return BadRequest(new { ok = false, reason = "invalid_name" });
        }

        if (string.IsNullOrWhiteSpace(email) || email.Length > 256 || !new EmailAddressAttribute().IsValid(email))
        {
            return BadRequest(new { ok = false, reason = "invalid_email" });
        }

        if (message.Length < MinMessageLength || message.Length > MaxMessageLength)
        {
            return BadRequest(new { ok = false, reason = "invalid_message" });
        }

        var recentThreshold = now.AddMinutes(-IpCooldownMinutes);
        var hasRecentFromIp = await _db.ContactMessages
            .AsNoTracking()
            .AnyAsync(m => m.SiteId == site.Id && m.SenderIp == ip && m.CreatedAt >= recentThreshold);
        if (hasRecentFromIp)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { ok = false, reason = "rate_limit" });
        }

        var ipCountToday = await _db.ContactMessages
            .AsNoTracking()
            .CountAsync(m => m.SiteId == site.Id && m.SenderIp == ip && m.CreatedAt >= todayUtc);
        if (ipCountToday >= MaxIpMessagesPerDay)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { ok = false, reason = "rate_limit" });
        }

        var siteCountToday = await _db.ContactMessages
            .AsNoTracking()
            .CountAsync(m => m.SiteId == site.Id && m.CreatedAt >= todayUtc);
        if (siteCountToday >= MaxSiteMessagesPerDayFree)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { ok = false, reason = "limit" });
        }

        var item = new ContactMessage
        {
            Id = Guid.NewGuid(),
            SiteId = site.Id,
            SiteOwnerUserId = site.OwnerUserId,
            SenderName = WebUtility.HtmlEncode(name),
            SenderEmail = WebUtility.HtmlEncode(email),
            MessageText = WebUtility.HtmlEncode(message),
            SenderIp = ip,
            CreatedAt = now,
            IsSpam = false
        };

        _db.ContactMessages.Add(item);
        await _db.SaveChangesAsync();

        return Ok(new { ok = true });
    }

    private string GetSenderIp()
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return ip.Length <= 64 ? ip : ip.Substring(0, 64);
    }
}
