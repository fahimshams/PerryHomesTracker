using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Data;

namespace PerryHomesTracker.Tests;

public static class DbContextTestHelpers
{
    public static PerryHomesDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<PerryHomesDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PerryHomesDbContext(options);
    }
}
