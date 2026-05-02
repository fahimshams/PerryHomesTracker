using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Data;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests.Unit;

public class HomesControllerTests
{
    [Fact]
    public async Task Create_Homes_ValidInput_RedirectsToIndex()
    {
        var stages = new List<Stage> { new() { Id = 1, Name = "Foundation", SortOrder = 1 } };
        var people = new List<Person>();
        var homes = new List<Home>();

        var mockCtx = new Mock<IPerryHomesDbContext>();
        mockCtx.Setup(c => c.Stages).Returns(stages.AsQueryable().BuildMockDbSet().Object);
        mockCtx.Setup(c => c.People).Returns(people.AsQueryable().BuildMockDbSet().Object);
        mockCtx.Setup(c => c.Homes).Returns(homes.AsQueryable().BuildMockDbSet().Object);
        mockCtx.Setup(c => c.Add(It.IsAny<object>()))
            .Returns((EntityEntry)null!)
            .Callback<object>(e =>
            {
                if (e is Home h)
                    homes.Add(h);
            });
        mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = new HomesController(mockCtx.Object);
        var home = new Home
        {
            AddressLine1 = "100 Main St",
            City = "Houston",
            State = "TX",
            Zip = "77002",
            StageId = 1
        };

        var result = await sut.Create(home);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(HomesController.Index), redirect.ActionName);
        mockCtx.Verify(c => c.Add(home), Times.Once);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_Homes_InvalidModel_ReturnsView_DoesNotSave()
    {
        var stages = new List<Stage> { new() { Id = 1, Name = "Foundation", SortOrder = 1 } };
        var people = new List<Person>();

        var mockCtx = new Mock<IPerryHomesDbContext>();
        mockCtx.Setup(c => c.Stages).Returns(stages.AsQueryable().BuildMockDbSet().Object);
        mockCtx.Setup(c => c.People).Returns(people.AsQueryable().BuildMockDbSet().Object);

        var sut = new HomesController(mockCtx.Object);
        sut.ModelState.AddModelError(nameof(Home.AddressLine1), "Address is required.");

        var home = new Home
        {
            AddressLine1 = "",
            City = "Houston",
            State = "TX",
            Zip = "77002",
            StageId = 1
        };

        var result = await sut.Create(home);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(home, view.Model);
        Assert.False(sut.ModelState.IsValid);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Homes_IdMismatch_ReturnsNotFound()
    {
        var mockCtx = new Mock<IPerryHomesDbContext>();
        var sut = new HomesController(mockCtx.Object);
        var edited = new Home
        {
            Id = 5,
            AddressLine1 = "200 Oak",
            City = "Houston",
            State = "TX",
            Zip = "77003",
            StageId = 1
        };

        var result = await sut.Edit(99, edited);

        Assert.IsType<NotFoundResult>(result);
        mockCtx.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
