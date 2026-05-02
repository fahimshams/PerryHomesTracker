using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests;

public class PeopleControllerTests
{
    [Fact]
    public async Task Create_Person_ValidInput_RedirectsToIndex_AndPersistsPerson()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var sut = new PeopleController(context);
        var person = new Person { FirstName = "Ada", LastName = "Lovelace", Email = "ada@example.com" };

        var result = await sut.Create(person);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(PeopleController.Index), redirect.ActionName);
        var saved = await context.People.SingleAsync();
        Assert.Equal("Ada", saved.FirstName);
    }

    [Fact]
    public async Task Create_Person_InvalidModel_ReturnsViewWithModelErrors()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var sut = new PeopleController(context);
        sut.ModelState.AddModelError(nameof(Person.FirstName), "First name is required.");

        var person = new Person { FirstName = "", LastName = "Test" };

        var result = await sut.Create(person);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(person, view.Model);
        Assert.False(sut.ModelState.IsValid);
        Assert.Empty(await context.People.ToListAsync());
    }

    [Fact]
    public async Task Edit_Person_IdMismatch_ReturnsNotFound()
    {
        await using var context = DbContextTestHelpers.CreateInMemoryContext();
        var person = new Person { FirstName = "Grace", LastName = "Hopper" };
        context.People.Add(person);
        await context.SaveChangesAsync();

        var sut = new PeopleController(context);
        var edited = new Person
        {
            Id = person.Id,
            FirstName = "Grace",
            LastName = "Hopper-Hall"
        };

        var result = await sut.Edit(person.Id + 50, edited);

        Assert.IsType<NotFoundResult>(result);
    }
}
