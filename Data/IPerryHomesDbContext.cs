using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Data;

public interface IPerryHomesDbContext
{
    DbSet<Home> Homes { get; }
    DbSet<Stage> Stages { get; }
    DbSet<Person> People { get; }
    DbSet<PurchaseInfo> PurchaseInfos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    EntityEntry Add(object entity);
    EntityEntry Update(object entity);
}
