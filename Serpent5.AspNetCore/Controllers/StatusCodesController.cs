using System.Globalization;
using System.Net;
using System.Text;
using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serpent5.AspNetCore.Builder;
using Yarp.ReverseProxy.Forwarder;

namespace Serpent5.AspNetCore.Controllers;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
[Route("/StatusCodes/{statusCode:int}")]
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class StatusCodesController : Controller
{
    private static readonly MediaTypeHeaderValue jsonMediaTypeHeaderValue = new("application/json");
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new("text/html");

    private static readonly Lazy<HttpMessageInvoker> lazyHttpMessageInvoker =
        new(CreateHttpMessageInvoker, LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly HtmlMarkupFormatter htmlMarkupFormatter = new MinifyMarkupFormatter
    {
        ShouldKeepStandardElements = true
    };

    // ReSharper disable once InconsistentNaming
    private readonly ClientUIBehaviorOptions clientUIBehaviorOptions;
    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly IMemoryCache? memoryCache;
    private readonly IHttpForwarder? httpForwarder;
    private readonly IViewEngine? viewEngine;

    public StatusCodesController(
        // ReSharper disable once InconsistentNaming
        IOptions<ClientUIBehaviorOptions> clientUIBehaviorOptionsAccessor,
        IWebHostEnvironment webHostEnvironment,
        IMemoryCache? memoryCache = null,
        IHttpForwarder? httpForwarder = null,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        // Specific type required for DI resolution.
        ICompositeViewEngine? compositeViewEngine = null)
    {
        ArgumentNullException.ThrowIfNull(clientUIBehaviorOptionsAccessor);
        ArgumentNullException.ThrowIfNull(webHostEnvironment);

        clientUIBehaviorOptions = clientUIBehaviorOptionsAccessor.Value;
        (this.webHostEnvironment, this.memoryCache, this.httpForwarder, viewEngine) = (webHostEnvironment, memoryCache, httpForwarder, compositeViewEngine);
    }

    public async Task<IActionResult> Index(int statusCode)
    {
        var statusCodeReExecuteFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        // If this endpoint has been requested outside of UseStatusCodePagesWithReExecute, hide it with a 404.
        if (statusCodeReExecuteFeature is null)
            statusCode = 404;

        var mediaTypeHeaderValues = MediaTypeHeaderValue.ParseList(Request.Headers.Accept);

        if (mediaTypeHeaderValues.Any(x => x.IsSubsetOf(jsonMediaTypeHeaderValue)))
            return Problem(statusCode: statusCode);

        if (httpForwarder is not null && clientUIBehaviorOptions.ServerAddress is not null)
        {
            if (await ForwardRequestAsync(clientUIBehaviorOptions.ServerAddress) is { } actionResult)
                return actionResult;
        }

        // ReSharper disable once InvertIf
        if (mediaTypeHeaderValues.Any(x => x.IsSubsetOf(htmlMediaTypeHeaderValue)))
        {
            if (statusCode == 404 && (statusCodeReExecuteFeature is null || Path.GetExtension(statusCodeReExecuteFeature.OriginalPath).Length == 0))
            {
                if (await GetNotFoundContentAsync() is { } contentResult)
                    return contentResult;
            }

            if (GetView(statusCode, statusCodeReExecuteFeature) is { } viewResult)
                return viewResult;
        }

        return StatusCode(statusCode);
    }

#pragma warning disable CA2000 // Dispose objects before losing scope
    private static HttpMessageInvoker CreateHttpMessageInvoker()
        => new(new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            UseProxy = false
        });
#pragma warning restore CA2000 // Dispose objects before losing scope

    private async Task<IActionResult?> ForwardRequestAsync(Uri serverAddress)
    {
        var forwarderError = await httpForwarder!.SendAsync(
            HttpContext,
            serverAddress.AbsoluteUri,
            lazyHttpMessageInvoker.Value,
            (ctx, httpRequestMessage) =>
            {
                if (ctx.Features.Get<IStatusCodeReExecuteFeature>() is not { } statusCodeReExecuteFeature)
                    return ValueTask.CompletedTask;

                httpRequestMessage.RequestUri = RequestUtilities.MakeDestinationAddress(
                    serverAddress.AbsoluteUri,
                    statusCodeReExecuteFeature.OriginalPath,
                    new QueryString(statusCodeReExecuteFeature.OriginalQueryString));

                return ValueTask.CompletedTask;
            });

        if (forwarderError is ForwarderError.None)
            return new EmptyResult();

        if (HttpContext.Features.Get<IForwarderErrorFeature>()?.Exception is { } ex and not OperationCanceledException)
            throw ex;

        return HttpContext.Response.HasStarted ? new EmptyResult() : StatusCode(500);
    }

    private async Task<ContentResult?> GetNotFoundContentAsync()
    {
        if (webHostEnvironment.WebRootFileProvider.GetFileInfo("index.html") is not { Exists: true } fileInfo)
            return null;

        if (memoryCache is null)
            return null;

        var htmlDocument = (await memoryCache.GetOrCreateAsync($"Serpent5.File:{fileInfo.Name}", async _ =>
        {
            await using var fileStream = fileInfo.CreateReadStream();
            return await new HtmlParser().ParseDocumentAsync(fileStream);
        }))!;

        if (!webHostEnvironment.IsDevelopment())
        {
            htmlDocument.Scripts.ToList()
                .ForEach(x => x.SetAttribute("nonce", HttpContext.GetNonce()));
        }

        var contentResult = Content(htmlDocument.ToHtml(htmlMarkupFormatter), htmlDocument.ContentType, Encoding.GetEncoding(htmlDocument.CharacterSet));
        contentResult.StatusCode = 200;
        return contentResult;
    }

    private ViewResult? GetView(int statusCode, IStatusCodeReExecuteFeature? statusCodeReExecuteFeature)
    {
        if (viewEngine is null)
            return null;

        string[] viewNameCandidates =
        {
            statusCode.ToString(CultureInfo.InvariantCulture),
            "Default"
        };

        if (viewNameCandidates.FirstOrDefault(x => viewEngine.FindView(ControllerContext, x, false).Success) is not { } viewName)
            return null;

        var viewResult = View(viewName, statusCodeReExecuteFeature);
        viewResult.StatusCode = statusCode;
        return viewResult;
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
