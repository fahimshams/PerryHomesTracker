using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Models;
using PerryHomesTracker.Tests.Helpers;

namespace PerryHomesTracker.Tests;

public class StagesControllerTests
{
    [Fact]
    public async Task Create_Stage_ValidInput_RedirectsToIndex_AndPersistsStage()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var sut = new StagesController(context);
        var stage = new Stage { Name = "Framing", SortOrder = 2, Description = "Walls up" };

        var result = await sut.Create(stage);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StagesController.Index), redirect.ActionName);
        var saved = await context.Stages.SingleAsync();
        Assert.Equal("Framing", saved.Name);
    }

    [Fact]
    public async Task Details_Stage_NullId_ReturnsNotFound()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var sut = new StagesController(context);

        var result = await sut.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_Stage_WithHomes_RedirectsToDelete_WithTempDataError()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var stage = new Stage { Name = "Closed", SortOrder = 9 };
        context.Stages.Add(stage);
        await context.SaveChangesAsync();
        context.Homes.Add(new Home
        {
            AddressLine1 = "1 Elm",
            City = "Houston",
            State = "TX",
            Zip = "77001",
            StageId = stage.Id
        });
        await context.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        var sut = new StagesController(context)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = new TempDataDictionary(httpContext, new MemoryTempDataProvider())
        };

        var result = await sut.DeleteConfirmed(stage.Id);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StagesController.Delete), redirect.ActionName);
        Assert.Equal(stage.Id, redirect.RouteValues!["id"]);
        Assert.Equal("Cannot delete a stage that is assigned to one or more homes.", sut.TempData["Error"]);
    }
}
