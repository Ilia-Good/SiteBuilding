using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;

namespace SiteBuilder.Middleware;

public class TrialMiddleware
{
    private readonly RequestDelegate _next;

    public TrialMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        var isBuilder = path.StartsWith("/builder");
        var isPublish = path.StartsWith("/api/publish");

        if (!isBuilder && !isPublish)
        {
            await _next(context);
            return;
        }

        if (path.StartsWith("/builder/trial-expired"))
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userIdRaw = context.User.FindFirstValue("app_user_id");
        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            await _next(context);
            return;
        }

        var userExists = await db.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/account/login");
            return;
        }

        var now = DateTime.UtcNow;
        var hasExpiredUnpaid = await db.Sites
            .AnyAsync(s => s.OwnerUserId == userId && (!s.IsPaid && s.ExpiresAt <= now || !s.IsActive));
        if (!hasExpiredUnpaid)
        {
            await _next(context);
            return;
        }

        if (isPublish)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Пробный период истёк. Оплатите сайт или отметьте его как оплаченный.");
            return;
        }

        context.Response.Redirect("/builder/trial-expired");
    }
}
