using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using PerryHomesTracker.Controllers;
using PerryHomesTracker.Models;

namespace PerryHomesTracker.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_Home_ReturnsViewResult()
    {
        var sut = new HomeController(NullLogger<HomeController>.Instance);

        var result = sut.Index();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_Home_ReturnsViewResult()
    {
        var sut = new HomeController(NullLogger<HomeController>.Instance);

        var result = sut.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_Home_ReturnsViewResult_WithRequestIdFromHttpContext()
    {
        var httpContext = new DefaultHttpContext { TraceIdentifier = "trace-test-123" };
        var sut = new HomeController(NullLogger<HomeController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };

        var result = sut.Error();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal("trace-test-123", model.RequestId);
        Assert.True(model.ShowRequestId);
    }
}
