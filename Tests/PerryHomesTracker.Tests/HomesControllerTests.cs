using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests;

public class HomesControllerTests
{
    [Fact]
    public async Task Create_Home_ValidInput_RedirectsToIndex_AndPersistsHome()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Foundation", SortOrder = 1 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();

        var sut = new HomesController(context);
        var home = new Home
        {
            AddressLine1 = "100 Main St",
            City = "Houston",
            State = "TX",
            Zip = "77002",
            StageId = stage.Id
        };

        var result = await sut.Create(home);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(HomesController.Index), redirect.ActionName);
        var saved = await context.Homes.SingleAsync();
        Assert.Equal("100 Main St", saved.AddressLine1);
        Assert.Equal(stage.Id, saved.StageId);
    }

    [Fact]
    public async Task Create_Home_InvalidModel_ReturnsViewWithModelErrors()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Foundation", SortOrder = 1 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();

        var sut = new HomesController(context);
        sut.ModelState.AddModelError(nameof(Home.AddressLine1), "Address is required.");

        var home = new Home
        {
            AddressLine1 = "",
            City = "Houston",
            State = "TX",
            Zip = "77002",
            StageId = stage.Id
        };

        var result = await sut.Create(home);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(home, view.Model);
        Assert.False(sut.ModelState.IsValid);
        Assert.Empty(await context.Homes.ToListAsync());
    }

    [Fact]
    public async Task Edit_Home_IdMismatch_ReturnsNotFound()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Foundation", SortOrder = 1 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();
        var home = new Home
        {
            AddressLine1 = "200 Oak",
            City = "Houston",
            State = "TX",
            Zip = "77003",
            StageId = stage.Id
        };
        context.Homes.Add(home);
        await context.SaveChangesAsync();

        var sut = new HomesController(context);
        var edited = new Home
        {
            Id = home.Id,
            AddressLine1 = "200 Oak Updated",
            City = "Houston",
            State = "TX",
            Zip = "77003",
            StageId = stage.Id
        };

        var result = await sut.Edit(home.Id + 999, edited);

        Assert.IsType<NotFoundResult>(result);
    }
}
