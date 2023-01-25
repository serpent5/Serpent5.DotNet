using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Moq;
using Serpent5.AspNetCore.Builder;
using Serpent5.AspNetCore.Controllers;
using Yarp.ReverseProxy.Forwarder;

namespace Serpent5.AspNetCore.Tests.Controllers;

public class StatusCodesControllerTests
{
    private const string anyFilenamePath = "/any.file";
    private const ForwarderError anyForwarderError = ForwarderError.Request;
    private static readonly Uri anyAbsoluteUri = new("https://example.com");

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public async Task ProducesStatusCode(int statusCode)
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment())
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature()
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(await statusCodesController.Index(statusCode));

        Assert.Equal(statusCode, statusCodeResult.StatusCode);
    }

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public async Task DirectRequest_ProducesStatusCode404(int statusCode)
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment())
        {
            ControllerContext = TestFakes.ControllerContext()
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(await statusCodesController.Index(statusCode));

        Assert.Equal(404, statusCodeResult.StatusCode);
    }

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public async Task ClientAcceptsJSON_ProducesProblemDetails(int statusCode)
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment())
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Application.Json } }
        };

        var objectResult = Assert.IsType<ObjectResult>(await statusCodesController.Index(statusCode));

        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(statusCode, problemDetails.Status);
    }

    [Fact]
    public async Task StatusCode404_HttpForwarder_ForwardsRequest()
    {
        var mockHttpForwarder = new Mock<IHttpForwarder>();

        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), httpForwarder: mockHttpForwarder.Object)
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature("/Example", "?Example=Value")
        };

        Assert.IsType<EmptyResult>(await statusCodesController.Index(404));

        mockHttpForwarder.Verify(httpForwarderSendAsync, Times.Once);
    }

    [Fact]
    public async Task StatusCode404_HttpForwarder_TransformsRequest()
    {
        var mockHttpForwarder = new Mock<IHttpForwarder>();
        var fakeServerAddress = new Uri("https://ui.example.com");

        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(fakeServerAddress), TestFakes.WebHostEnvironment(), httpForwarder: mockHttpForwarder.Object)
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature("/Example", "?Example=Value")
        };

        await statusCodesController.Index(404);

        var fakeHttpRequestMessage = await TransformHttpForwarderRequestAsync(mockHttpForwarder, statusCodesController.HttpContext);

        var expectedRequestUriBuilder = new UriBuilder(fakeServerAddress)
        {
            Path = "Example",
            Query = "Example=Value"
        };

        Assert.Equal(expectedRequestUriBuilder.Uri, fakeHttpRequestMessage.RequestUri);
    }

    [Fact]
    public async Task StatusCode404_DirectRequest_HttpForwarder_DoesNotTransformRequest()
    {
        var mockHttpForwarder = new Mock<IHttpForwarder>();
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), httpForwarder: mockHttpForwarder.Object)
        {
            ControllerContext = TestFakes.ControllerContext()
        };

        await statusCodesController.Index(404);

        var fakeHttpRequestMessage = await TransformHttpForwarderRequestAsync(mockHttpForwarder, statusCodesController.HttpContext);

        Assert.Null(fakeHttpRequestMessage.RequestUri);
    }

    [Fact]
    public async Task StatusCode404_HttpForwarder_ForwarderErrorException_ThrowsErrorAsException()
    {
        var mockHttpForwarder = new Mock<IHttpForwarder>();

        mockHttpForwarder.Setup(httpForwarderSendAsync).ReturnsAsync(anyForwarderError);

        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), httpForwarder: mockHttpForwarder.Object)
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature()
        };

#pragma warning disable CA2201 // Do not raise reserved exception types
        var fakeForwarderErrorFeature = Mock.Of<IForwarderErrorFeature>(x => x.Exception == new Exception());
#pragma warning restore CA2201 // Do not raise reserved exception types

        statusCodesController.HttpContext.Features.Set(fakeForwarderErrorFeature);

        Assert.Same(
            fakeForwarderErrorFeature.Exception,
            await Assert.ThrowsAsync<Exception>(() => statusCodesController.Index(404)));
    }

    [Fact]
    public async Task StatusCode404_HttpForwarder_ForwarderErrorWithoutException_ProducesStatusCode500()
    {
        var mockHttpForwarder = new Mock<IHttpForwarder>();

        mockHttpForwarder.Setup(httpForwarderSendAsync).ReturnsAsync(anyForwarderError);

        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), httpForwarder: mockHttpForwarder.Object)
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature()
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(await statusCodesController.Index(400));

        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task StatusCode404_ClientAcceptsHTML_IndexHTMLExists_ProducesHTML()
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), CreateFakeWebHostEnvironmentWithFileProvider(CreateFakeFileProvider()), TestFakes.MemoryCache())
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var contentResult = Assert.IsType<ContentResult>(await statusCodesController.Index(404));

        Assert.Equal(200, contentResult.StatusCode);
        Assert.Equal("text/html; charset=utf-8", contentResult.ContentType);
        Assert.Equal("<html><head></head><body></body></html>", contentResult.Content);
    }

    [Fact]
    public async Task StatusCode404_ClientAcceptsHTML_IndexHTMLExists_AddsScriptNoncesToHTML()
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(),
            CreateFakeWebHostEnvironmentWithFileProvider(CreateFakeFileProvider("<body><script></script></body>")),
            TestFakes.MemoryCache())
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var contentResult = Assert.IsType<ContentResult>(await statusCodesController.Index(404));
        var expectedNonce = statusCodesController.HttpContext.GetNonce();

        Assert.Equal(
            $@"<html><head></head><body><script nonce=""{expectedNonce}""></script></body></html>",
            contentResult.Content);
    }

    [Fact]
    public async Task StatusCode404_ClientAcceptsHTML_IndexHTMLExists_CachesIndexHTMLContents()
    {
        var fakeWebHostEnvironment = CreateFakeWebHostEnvironmentWithFileProvider(CreateFakeFileProvider());

        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), fakeWebHostEnvironment, TestFakes.MemoryCache())
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        await statusCodesController.Index(404);
        await statusCodesController.Index(404);

        Mock.Get(fakeWebHostEnvironment.WebRootFileProvider.GetFileInfo(string.Empty))
            .Verify(x => x.CreateReadStream(), Times.Once);
    }

    [Fact]
    public async Task StatusCode404_FileRequest_ClientAcceptsHTML_IndexHTMLExists_ProducesStatusCode404()
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), CreateFakeWebHostEnvironmentWithFileProvider(CreateFakeFileProvider()))
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(anyFilenamePath),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var statusCodeResult = Assert.IsType<StatusCodeResult>(await statusCodesController.Index(404));

        Assert.Equal(404, statusCodeResult.StatusCode);
    }

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public async Task ClientAcceptsHTML_StatusCodeViewExists_ProducesStatusCodeView(int statusCode)
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), compositeViewEngine: CreateFakeCompositeViewEngine(statusCode.ToString(CultureInfo.InvariantCulture)))
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var viewResult = Assert.IsType<ViewResult>(await statusCodesController.Index(statusCode));

        Assert.Equal(statusCode, viewResult.StatusCode);
        Assert.Equal(statusCode.ToString(CultureInfo.InvariantCulture), viewResult.ViewName);
    }

    [Theory]
    [ClassData(typeof(TestStatusCodes))]
    public async Task ClientAcceptsHTML_StatusCodeViewDoesNotExist_DefaultViewExists_ProducesDefaultView(int statusCode)
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), compositeViewEngine: CreateFakeCompositeViewEngine("Default"))
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var viewResult = Assert.IsType<ViewResult>(await statusCodesController.Index(statusCode));

        Assert.Equal(statusCode, viewResult.StatusCode);
        Assert.Equal("Default", viewResult.ViewName);
    }

    [Fact]
    public async Task StatusCode404_FileRequest_ClientAcceptsHTML_StatusCodeViewExists_ProducesStatusCodeView404()
    {
        var statusCodesController = new StatusCodesController(
            CreateFakeStatusCodesOptions(), TestFakes.WebHostEnvironment(), compositeViewEngine: CreateFakeCompositeViewEngine("404"))
        {
            ControllerContext = CreateFakeControllerContextWithStatusCodeReExecuteFeature(anyFilenamePath),
            Request = { Headers = { Accept = MediaTypeNames.Text.Html } }
        };

        var viewResult = Assert.IsType<ViewResult>(await statusCodesController.Index(404));

        Assert.Equal(404, viewResult.StatusCode);
        Assert.Equal("404", viewResult.ViewName);
    }

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

    private static IOptions<ClientUIBehaviorOptions> CreateFakeStatusCodesOptions(Uri? serverAddress = null)
        => TestFakes.Options(new ClientUIBehaviorOptions { ServerAddress = serverAddress ?? anyAbsoluteUri });

    private static readonly Expression<Func<IHttpForwarder, ValueTask<ForwarderError>>> httpForwarderSendAsync =
        x => x.SendAsync(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<HttpMessageInvoker>(), It.IsAny<ForwarderRequestConfig>(), It.IsAny<HttpTransformer>());

    // ReSharper disable once SuggestBaseTypeForParameter
    private static async Task<HttpRequestMessage> TransformHttpForwarderRequestAsync(Mock<IHttpForwarder> mockHttpForwarder, HttpContext httpContext)
    {
        var httpRequestMessage = new HttpRequestMessage();
        var httpTransformer = Assert.IsAssignableFrom<HttpTransformer>(Assert.Single(mockHttpForwarder.Invocations, x => x.Method.Name == nameof(IHttpForwarder.SendAsync)).Arguments[^1]);

        await httpTransformer.TransformRequestAsync(httpContext, httpRequestMessage, "__UNUSED__");

        return httpRequestMessage;
    }

    private static IWebHostEnvironment CreateFakeWebHostEnvironmentWithFileProvider(IFileProvider? fileProvider = null)
    {
        var webHostEnvironment = TestFakes.WebHostEnvironment();
        webHostEnvironment.WebRootFileProvider = fileProvider ?? Mock.Of<IFileProvider>();
        return webHostEnvironment;
    }

    private static IFileProvider CreateFakeFileProvider(string? fileContents = null, string? fileName = null)
    {
        var fakeFileInfo = Mock.Of<IFileInfo>(x =>
            x.Exists &&
            x.Name == (fileName ?? string.Empty) &&
            x.CreateReadStream() == new MemoryStream(Encoding.UTF8.GetBytes(fileContents ?? string.Empty)));

        return Mock.Of<IFileProvider>(x => x.GetFileInfo(It.IsAny<string>()) == fakeFileInfo);
    }

    private static ICompositeViewEngine CreateFakeCompositeViewEngine(params string[] viewNames)
    {
        var mockCompositeViewEngine = new Mock<ICompositeViewEngine>();

        mockCompositeViewEngine
            .Setup(x => x.FindView(It.IsAny<ControllerContext>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns<ControllerContext, string, bool>((_, viewName, _) =>
                viewNames.Contains(viewName)
                    ? ViewEngineResult.Found(viewName, Mock.Of<IView>())
                    : ViewEngineResult.NotFound(viewName, Enumerable.Empty<string>())
            );

        return mockCompositeViewEngine.Object;
    }

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    private sealed class TestStatusCodes : IEnumerable<object[]>
    {
        private static readonly List<object[]> testStatusCodes = new()
        {
            new object[] { 200 },
            new object[] { 400 },
            new object[] { 500 }
        };

        public IEnumerator<object[]> GetEnumerator() => testStatusCodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
}
