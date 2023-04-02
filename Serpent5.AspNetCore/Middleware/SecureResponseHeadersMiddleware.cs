using System.Globalization;
using System.Net.Mime;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serpent5.AspNetCore.Builder;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

namespace Serpent5.AspNetCore.Middleware;

internal sealed class SecureResponseHeadersMiddleware
{
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new(MediaTypeNames.Text.Html);

    private readonly RequestDelegate nextMiddleware;

    public SecureResponseHeadersMiddleware(RequestDelegate nextMiddleware)
        => this.nextMiddleware = nextMiddleware;

    [UsedImplicitly]
    // ReSharper disable once InconsistentNaming
    public Task InvokeAsync(HttpContext httpContext, IOptionsSnapshot<ClientUIBehaviorOptions> clientUIBehaviorOptionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.OnStarting(static stateAsObject =>
        {
            // ReSharper disable once InconsistentNaming
            var (httpContext, clientUIServerAddress) = (ValueTuple<HttpContext, Uri?>)stateAsObject;
            var httpResponse = httpContext.Response;
            var httpResponseHeaders = httpResponse.Headers;

            httpResponseHeaders.XContentTypeOptions = "nosniff";

            if (httpResponse.GetTypedHeaders().ContentType?.IsSubsetOf(htmlMediaTypeHeaderValue) != true)
                return Task.CompletedTask;

            // https://web.dev/strict-csp
            // https://csp.withgoogle.com/docs/strict-csp.html
            var cspBuilder = new StringBuilder("base-uri 'self'; frame-ancestors 'none'; object-src 'none'; ");

            cspBuilder.Append("script-src ");

            if (httpContext.TryGetNonce(out var nonceValue))
                cspBuilder.Append(CultureInfo.InvariantCulture, $"'strict-dynamic' 'nonce-{nonceValue}' 'unsafe-inline' https:");
            else
                cspBuilder.Append("'none'");

            if (clientUIServerAddress is not null)
                cspBuilder.Append("; trusted-types default angular angular#bundler");

            cspBuilder.Append("; require-trusted-types-for 'script'");

            httpResponse.Headers["Content-Security-Policy"] = cspBuilder.ToString();

            // https://github.com/w3c/webappsec-permissions-policy/blob/main/features.md
            httpResponseHeaders["Permissions-Policy"] =
                "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), hid=(), idle-detection=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), serial=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=()";

            httpResponseHeaders["Referrer-Policy"] = "strict-origin-when-cross-origin";

            return Task.CompletedTask;
        }, (httpContext, clientUIBehaviorOptionsAccessor.Value.ServerAddress));

        return nextMiddleware(httpContext);
    }
}
