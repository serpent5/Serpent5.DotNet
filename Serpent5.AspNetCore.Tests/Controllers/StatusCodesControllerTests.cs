using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Serpent5.AspNetCore.Controllers;

namespace Serpent5.AspNetCore.Tests.Controllers;

using static MediaTypeNames;
using static StatusCodes;

public class StatusCodesControllerTests
{
    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public void Produces_StatusCode(int statusCode)
    {
        var statusCodesController = new StatusCodesController
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature()
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(statusCodesController.Index(statusCode));

        Assert.Equal(statusCode, statusCodeResult.StatusCode);
    }

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public void Produces_StatusCode_404_When_Request_Is_Direct(int statusCode)
    {
        var statusCodesController = new StatusCodesController
        {
            ControllerContext = TestFakes.ControllerContext()
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(statusCodesController.Index(statusCode));

        Assert.Equal(Status404NotFound, statusCodeResult.StatusCode);
    }

    [Fact]
    public void Restores_Original_Request_Path_And_QueryString()
    {
        const string fakeOriginalPath = "/fake/path";
        const string fakeOriginalQueryString = "?fake=value";

        var statusCodesController = new StatusCodesController
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(fakeOriginalPath, fakeOriginalQueryString)
        };

        statusCodesController.Index(Status200OK);

        Assert.Equal(fakeOriginalPath, statusCodesController.HttpContext.Request.Path);
        Assert.Equal(fakeOriginalQueryString, statusCodesController.HttpContext.Request.QueryString.Value);
    }

    // ReSharper disable once InconsistentNaming
    public class Client_Accepts_JSON
    {
        [Theory]
        [ClassData(typeof(TestStatusCodes))]
        public void Produces_ProblemDetails(int statusCode)
        {
            var statusCodesController = new StatusCodesController
            {
                ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
                Request = { Headers = { Accept = Application.Json } }
            };

            var objectResult = Assert.IsType<ObjectResult>(statusCodesController.Index(statusCode));

            var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal(statusCode, problemDetails.Status);
        }
    }

    // ReSharper disable once InconsistentNaming
    public class Client_Accepts_HTML
    {
        [Theory]
        [ClassData(typeof(TestStatusCodes))]
        public void Produces_StatusCode_View(int statusCode)
        {
            var statusCodesController = new StatusCodesController(CreateFakeCompositeViewEngine(statusCode.ToString(CultureInfo.InvariantCulture)))
            {
                ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
                Request = { Headers = { Accept = Text.Html } }
            };

            var viewResult = Assert.IsType<ViewResult>(statusCodesController.Index(statusCode));

            Assert.Equal(statusCode, viewResult.StatusCode);
            Assert.Equal(statusCode.ToString(CultureInfo.InvariantCulture), viewResult.ViewName);
        }

        [Theory]
        [ClassData(typeof(TestStatusCodes))]
        public void Produces_Default_View_When_StatusCode_View_Does_Not_Exist(int statusCode)
        {
            var statusCodesController = new StatusCodesController(CreateFakeCompositeViewEngine("Default"))
            {
                ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
                Request = { Headers = { Accept = Text.Html } }
            };

            var viewResult = Assert.IsType<ViewResult>(statusCodesController.Index(statusCode));

            Assert.Equal(statusCode, viewResult.StatusCode);
            Assert.Equal("Default", viewResult.ViewName);
        }

        [Theory]
        [ClassData(typeof(TestStatusCodes))]
        public void Produces_StatusCode_When_ViewEngine_Is_Not_Available(int statusCode)
        {
            var statusCodesController = new StatusCodesController
            {
                ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
                Request = { Headers = { Accept = Text.Html } }
            };

            var statusCodeResult = Assert.IsType<StatusCodeResult>(statusCodesController.Index(statusCode));

            Assert.Equal(statusCode, statusCodeResult.StatusCode);
        }

        [Theory]
        [ClassData(typeof(TestStatusCodes))]
        public void Produces_StatusCode_When_StatusCode_View_And_Default_Views_Do_Not_Exist(int statusCode)
        {
            var statusCodesController = new StatusCodesController(CreateFakeCompositeViewEngine())
            {
                ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
                Request = { Headers = { Accept = Text.Html } }
            };

            var statusCodeResult = Assert.IsType<StatusCodeResult>(statusCodesController.Index(statusCode));

            Assert.Equal(statusCode, statusCodeResult.StatusCode);
        }
    }

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    [ExcludeFromCodeCoverage]
    private sealed class TestStatusCodes : IEnumerable<object[]>
    {
        private static readonly List<object[]> testStatusCodes = new()
        {
            new object[] { Status200OK },
            new object[] { Status400BadRequest },
            new object[] { Status500InternalServerError }
        };

        public IEnumerator<object[]> GetEnumerator() => testStatusCodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
#pragma warning restore CA1812 // Avoid uninstantiated internal classes

    private static ControllerContext CreateFakeControllerContextWithStatusCodeReExecuteFeature(string? originalPath = null, string? originalQueryString = null)
    {
        var fakeControllerContext = TestFakes.ControllerContext();

        fakeControllerContext.HttpContext.Features.Set<IStatusCodeReExecuteFeature>(
            new StatusCodeReExecuteFeature
            {
                OriginalPath = originalPath ?? string.Empty,
                OriginalQueryString = originalQueryString ?? string.Empty
            });

        return fakeControllerContext;
    }

    private static ICompositeViewEngine CreateFakeCompositeViewEngine(params string[] viewNames)
    {
        var mockCompositeViewEngine = new Mock<ICompositeViewEngine>();

        mockCompositeViewEngine
            .Setup(x => x.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns<ControllerContext, string, bool>((_, viewName, _) =>
                viewNames.Contains(viewName)
                    ? ViewEngineResult.Found(viewName, Mock.Of<IView>())
                    : ViewEngineResult.NotFound(viewName, Enumerable.Empty<string>()));

        return mockCompositeViewEngine.Object;
    }
}
