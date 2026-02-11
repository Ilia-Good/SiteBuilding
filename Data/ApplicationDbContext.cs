using Microsoft.EntityFrameworkCore;
using SiteBuilder.Models;

namespace SiteBuilder.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<SiteDailyUsage> SiteDailyUsages => Set<SiteDailyUsage>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.GoogleId)
            .IsUnique();

        modelBuilder.Entity<Site>()
            .HasIndex(s => s.SiteName)
            .IsUnique();

        modelBuilder.Entity<Site>()
            .HasIndex(s => new { s.OwnerUserId, s.SiteName })
            .IsUnique();

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Sites)
            .WithOne(s => s.Owner!)
            .HasForeignKey(s => s.OwnerUserId);

        modelBuilder.Entity<SiteDailyUsage>()
            .HasIndex(u => new { u.SiteId, u.Date })
            .IsUnique();

        modelBuilder.Entity<Site>()
            .HasMany(s => s.DailyUsages)
            .WithOne(u => u.Site!)
            .HasForeignKey(u => u.SiteId);

        modelBuilder.Entity<ContactMessage>()
            .HasIndex(m => new { m.SiteId, m.CreatedAt });

        modelBuilder.Entity<ContactMessage>()
            .HasIndex(m => new { m.SiteId, m.SenderIp, m.CreatedAt });

        modelBuilder.Entity<Site>()
            .HasMany(s => s.ContactMessages)
            .WithOne(m => m.Site!)
            .HasForeignKey(m => m.SiteId);
    }
}
