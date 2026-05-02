using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests.Unit;

public class StagesControllerTests
{
    [Fact]
    public async Task Create_Stages_ValidInput_RedirectsToIndex()
    {
        var mockCtx = new Mock<IPerryHomesDbContext>();
        mockCtx.Setup(c => c.Add(It.IsAny<object>()))
            .Returns((EntityEntry)null!);
        mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new StagesController(mockCtx.Object);
        var stage = new Stage { Name = "Framing", SortOrder = 2, Description = "Walls up" };

        var result = await sut.Create(stage);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StagesController.Index), redirect.ActionName);
        mockCtx.Verify(c => c.Add(stage), Times.Once);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Details_Stages_NullId_ReturnsNotFound()
    {
        var mockCtx = new Mock<IPerryHomesDbContext>();
        var sut = new StagesController(mockCtx.Object);

        var result = await sut.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_Stages_InvalidModel_ReturnsView_DoesNotSave()
    {
        var mockCtx = new Mock<IPerryHomesDbContext>();
        var sut = new StagesController(mockCtx.Object);
        sut.ModelState.AddModelError(nameof(Stage.Name), "The Name field is required.");

        var stage = new Stage { Name = "", SortOrder = 1 };

        var result = await sut.Create(stage);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(stage, view.Model);
        Assert.False(sut.ModelState.IsValid);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Stages_IdMismatch_ReturnsNotFound()
    {
        var mockCtx = new Mock<IPerryHomesDbContext>();
        var sut = new StagesController(mockCtx.Object);
        var stage = new Stage { Id = 3, Name = "Closed", SortOrder = 9 };

        var result = await sut.Edit(50, stage);

        Assert.IsType<NotFoundResult>(result);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
