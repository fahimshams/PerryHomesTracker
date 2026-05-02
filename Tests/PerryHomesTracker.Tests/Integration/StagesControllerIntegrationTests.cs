using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests.Integration;

public class StagesControllerIntegrationTests
{
    [Fact]
    public async Task Index_Stages_Returns200_AndListsSeededStage()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        _ = factory.Server;

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
        db.Stages.Add(new Stage { Name = "Inspection", SortOrder = 3 });
        await db.SaveChangesAsync();

        var client = factory.CreateClient();
        var response = await client.GetAsync("/Stages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Inspection", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Create_Stages_ValidForm_RedirectsToIndex_AndPersists()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        _ = factory.Server;

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var form = new Dictionary<string, string>
        {
            ["Id"] = "0",
            ["Name"] = "Final Walk",
            ["SortOrder"] = "4",
            ["Description"] = "Punch list"
        };

        var response = await AntiforgeryFormPost.PostWithTokenAsync(
            client,
            "/Stages/Create",
            "/Stages/Create",
            form);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? "";
        Assert.True(
            location is "/Stages" or "/Stages/Index",
            $"Unexpected redirect: {location}");

        await using var verifyScope = factory.Services.CreateAsyncScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
        var saved = await verifyDb.Stages.SingleAsync();
        Assert.Equal("Final Walk", saved.Name);
    }

    [Fact]
    public async Task DeleteConfirmed_Stages_WithHomes_RedirectsBackToDelete_WithTempData()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        _ = factory.Server;

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PerryHomesDbContext>();
        var stage = new Stage { Name = "Blocked", SortOrder = 99 };
        db.Stages.Add(stage);
        await db.SaveChangesAsync();
        db.Homes.Add(new Home
        {
            AddressLine1 = "1 Conflict",
            City = "Houston",
            State = "TX",
            Zip = "77008",
            StageId = stage.Id
        });
        await db.SaveChangesAsync();

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var deletePath = $"/Stages/Delete/{stage.Id}";
        var response = await AntiforgeryFormPost.PostWithTokenAsync(
            client,
            deletePath,
            deletePath,
            new Dictionary<string, string>());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.OriginalString ?? "";
        Assert.Contains("/Stages/Delete/", location, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(stage.Id.ToString(), location, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Details_Stages_UnknownId_Returns404()
    {
        await using var factory = new PerryHomesWebApplicationFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/Stages/Details/424242");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
