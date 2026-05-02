using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Data;

public class PerryHomesDbContext : DbContext, IPerryHomesDbContext
{
    public PerryHomesDbContext(DbContextOptions<PerryHomesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Home> Homes => Set<Home>();
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PurchaseInfo> PurchaseInfos => Set<PurchaseInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Home>(e =>
        {
            e.HasOne(h => h.Stage)
                .WithMany(s => s.Homes)
                .HasForeignKey(h => h.StageId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(h => h.PrimaryContact)
                .WithMany(p => p.HomesAsPrimaryContact)
                .HasForeignKey(h => h.PrimaryContactId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PurchaseInfo>(e =>
        {
            e.HasOne(p => p.Home)
                .WithOne(h => h.PurchaseInfo)
                .HasForeignKey<PurchaseInfo>(p => p.HomeId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(p => p.HomeId).IsUnique();
        });
    }
}
