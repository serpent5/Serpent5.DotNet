using System.Text;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

// ReSharper disable once InconsistentNaming
internal class SwaggerUIOptionsSetup : IConfigureOptions<SwaggerUIOptions>
{
    private static readonly HtmlParser htmlParser = new();

    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMemoryCache memoryCache;

    public SwaggerUIOptionsSetup(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        => (this.httpContextAccessor, this.memoryCache) = (httpContextAccessor, memoryCache);

    // ReSharper disable once InconsistentNaming
    public void Configure(SwaggerUIOptions swaggerUIOptions)
    {
        var getIndexStream = swaggerUIOptions.IndexStream;

        swaggerUIOptions.IndexStream = () =>
        {
            var indexStream = getIndexStream();

            if (httpContextAccessor.HttpContext is not { } httpContext
                || memoryCache.GetOrCreate("Serpent5.Host:Swagger", _ => htmlParser.ParseDocument(indexStream)) is not { } htmlDocument)
            {
                return indexStream;
            }

            if (httpContext.Features.Get<SecureResponseHeadersFeature>() is { } secureResponseHeadersFeature)
                secureResponseHeadersFeature.RequireTrustedTypes = false;

            return new MemoryStream(Encoding.UTF8.GetBytes(htmlDocument.ToMinifiedHtmlWithScriptNonce(httpContext.GetNonce)));
        };
    }
}
