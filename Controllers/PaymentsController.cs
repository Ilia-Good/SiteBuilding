using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;

namespace SiteBuilder.Controllers;

[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public PaymentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("mark-paid/{siteId:guid}")]
    public async Task<IActionResult> MarkPaid(Guid siteId)
    {
        var userIdRaw = User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return Unauthorized();
        }

        var site = await _db.Sites.FirstOrDefaultAsync(s => s.Id == siteId && s.OwnerUserId == userId);
        if (site is null)
        {
            return NotFound();
        }

        site.IsPaid = true;
        await _db.SaveChangesAsync();

        return Ok(new { siteId = site.Id, isPaid = site.IsPaid });
    }
}
