using System.Net.Mime;
using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Forwarder;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

using static MediaTypeNames;

/// <summary>
/// <see cref="IEndpointRouteBuilder"/> extension methods for adding fallback endpoints.
/// </summary>
public static class FallbackEndpointRouteBuilderExtensions
{
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new(Text.Html);

    /// <summary>
    /// Adds a fallback endpoint to the <see cref="IEndpointRouteBuilder" /> that proxies the request to <paramref name="serverAddress" />.
    /// </summary>
    /// <param name="endpointRouteBuilder">The <see cref="IEndpointRouteBuilder"/> to add the endpoint to.</param>
    /// <param name="serverAddress">The absolute URL of the server.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    public static IEndpointConventionBuilder MapFallbackToServer(this IEndpointRouteBuilder endpointRouteBuilder, string serverAddress)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(serverAddress);

        return endpointRouteBuilder
            .MapForwarder("{**path}", serverAddress, ForwarderRequestConfig.Empty, new FallbackHttpTransformer())
            .AllowAnonymous();
    }

    /// <summary>
    /// Adds a fallback endpoint to the <see cref="IEndpointRouteBuilder" /> that serves the contents of <c>index.html</c>.
    /// </summary>
    /// <param name="endpointRouteBuilder">The <see cref="IEndpointRouteBuilder"/> to add the endpoint to.</param>
    /// <returns>An <see cref="IEndpointConventionBuilder"/> that can be used to further customize the endpoint.</returns>
    // ReSharper disable once InconsistentNaming
    public static IEndpointConventionBuilder MapFallbackToIndexHTML(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        ArgumentNullException.ThrowIfNull(endpointRouteBuilder);

        return endpointRouteBuilder
            .MapFallback(static async (HttpContext ctx, IWebHostEnvironment webHostEnvironment, IMemoryCache memoryCache) =>
            {
                var mediaTypeHeaderValues = MediaTypeHeaderValue.ParseList(ctx.Request.Headers.Accept);

                if (!mediaTypeHeaderValues.Any(x => x.IsSubsetOf(htmlMediaTypeHeaderValue)))
                    return Results.NotFound();

                var htmlDocument = await memoryCache
                    .GetOrCreateAsync("Serpent5.Host:index.html", async _ =>
                    {
                        if (webHostEnvironment.WebRootFileProvider.GetFileInfo("index.html") is not { Exists: true } fileInfo)
                            return null;

                        var fileStream = fileInfo.CreateReadStream();

                        await using (fileStream.ConfigureAwait(false))
                        {
                            return await new HtmlParser().ParseDocumentAsync(fileStream, ctx.RequestAborted)
                                .ConfigureAwait(false);
                        }
                    })
                    .ConfigureAwait(false);

                if (htmlDocument is null)
                    return Results.NotFound();

                return Results.Content(
                    htmlDocument.ToMinifiedHtmlWithScriptNonce(ctx.GetNonce),
                    htmlDocument.ContentType,
                    Encoding.GetEncoding(htmlDocument.CharacterSet));
            })
            .WithMetadata(new HttpMethodMetadata(new[] { HttpMethods.Head, HttpMethods.Get }))
            .AllowAnonymous();
    }

    private class FallbackHttpTransformer : HttpTransformer
    {
        private static readonly HtmlParser htmlParser = new();

        public override async ValueTask<bool> TransformResponseAsync(HttpContext ctx, HttpResponseMessage? httpResponseMessage, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ctx);

            await base.TransformResponseAsync(ctx, httpResponseMessage, cancellationToken)
                .ConfigureAwait(false);

            if (httpResponseMessage is not { IsSuccessStatusCode: true, Content.Headers.ContentType: { } contentType }
                || !htmlMediaTypeHeaderValue.IsSubsetOf(MediaTypeHeaderValue.Parse(contentType.MediaType)))
            {
                return true;
            }

            var httpResponseContentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            using var htmlDocument = await htmlParser.ParseDocumentAsync(httpResponseContentStream, cancellationToken)
                .ConfigureAwait(false);

            ctx.Response.ContentLength = null;

            await ctx.Response.WriteAsync(htmlDocument.ToMinifiedHtmlWithScriptNonce(ctx.GetNonce), cancellationToken)
                .ConfigureAwait(false);

            return false;
        }
    }
}
