using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;

namespace SiteBuilder.Services;

public class TrialCheckService : BackgroundService
{
    private const int TrialHours = 48;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TrialCheckService> _logger;

    public TrialCheckService(IServiceProvider serviceProvider, ILogger<TrialCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrialCheckService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndUpdateExpiredTrials();
                // Run check once per day (every 24 hours)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrialCheckService");
                // Wait before retrying in case of error
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("TrialCheckService stopping.");
    }

    private async Task CheckAndUpdateExpiredTrials()
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;

        // Find all unpaid sites that have trial expired and are still marked as active
        var expiredSites = await db.Sites
            .Where(s => !s.IsPaid && s.ExpiresAt <= now && s.IsActive)
            .ToListAsync();

        if (expiredSites.Count > 0)
        {
            foreach (var site in expiredSites)
            {
                site.IsActive = false;
                _logger.LogInformation($"Deactivated trial site: {site.SiteName} (Owner: {site.OwnerUserId})");
            }

            await db.SaveChangesAsync();
            _logger.LogInformation($"Updated {expiredSites.Count} expired trial sites.");
        }
    }
}
