using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests.Integration;

public class HomesControllerIntegrationTests
{
    [Fact]
    public async Task Index_Homes_Returns200_AndRendersSeededHome()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        _ = factory.Server;

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
        var stage = new Stage { Name = "Foundation", SortOrder = 1 };
        db.Stages.Add(stage);
        await db.SaveChangesAsync();
        db.Homes.Add(new Home
        {
            AddressLine1 = "500 Cedar",
            City = "Houston",
            State = "TX",
            Zip = "77006",
            StageId = stage.Id
        });
        await db.SaveChangesAsync();

        var client = factory.CreateClient();
        var response = await client.GetAsync("/Homes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("500 Cedar", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_Homes_Returns200_AndRendersSeededStageInLookup()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        _ = factory.Server;

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
        var stage = new Stage { Name = "Build", SortOrder = 2 };
        db.Stages.Add(stage);
        await db.SaveChangesAsync();

        var client = factory.CreateClient();
        var response = await client.GetAsync("/Homes/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(stage.Name, html, StringComparison.Ordinal);
        Assert.Contains(stage.Id.ToString(), html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Details_Homes_UnknownId_Returns404()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Homes/Details/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
