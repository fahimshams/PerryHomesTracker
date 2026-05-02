using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests;

public class PurchaseInfosControllerTests
{
    [Fact]
    public async Task Create_PurchaseInfo_ValidInput_RedirectsToIndex_AndPersists()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Sold", SortOrder = 10 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();
        var home = new Home
        {
            AddressLine1 = "9 Pine",
            City = "Houston",
            State = "TX",
            Zip = "77004",
            StageId = stage.Id
        };
        context.Homes.Add(home);
        await context.SaveChangesAsync();

        var sut = new PurchaseInfosController(context);
        var purchase = new PurchaseInfo
        {
            HomeId = home.Id,
            ContractDate = new DateTime(2025, 1, 1),
            PurchasePrice = 350_000m
        };

        var result = await sut.Create(purchase);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PurchaseInfosController.Index), redirect.ActionName);
        var saved = await context.PurchaseInfos.SingleAsync();
        Assert.Equal(home.Id, saved.HomeId);
        Assert.Equal(350_000m, saved.PurchasePrice);
    }

    [Fact]
    public async Task Details_PurchaseInfo_NullId_ReturnsNotFound()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var sut = new PurchaseInfosController(context);

        var result = await sut.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_PurchaseInfo_DuplicateHomeId_ReturnsViewWithModelError()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Sold", SortOrder = 10 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();
        var home = new Home
        {
            AddressLine1 = "10 Maple",
            City = "Houston",
            State = "TX",
            Zip = "77005",
            StageId = stage.Id
        };
        context.Homes.Add(home);
        await context.SaveChangesAsync();
        context.PurchaseInfos.Add(new PurchaseInfo { HomeId = home.Id, PurchasePrice = 100m });
        await context.SaveChangesAsync();

        var sut = new PurchaseInfosController(context);
        var duplicate = new PurchaseInfo { HomeId = home.Id, PurchasePrice = 200m };

        var result = await sut.Create(duplicate);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(duplicate, view.Model);
        Assert.False(sut.ModelState.IsValid);
        Assert.True(sut.ModelState.ContainsKey(nameof(PurchaseInfo.HomeId)));
        Assert.Single(await context.PurchaseInfos.ToListAsync());
    }
}
