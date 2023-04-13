using System.Globalization;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

using static MediaTypeNames;

internal sealed class SecureResponseHeadersMiddleware
{
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new(Text.Html);

    private readonly RequestDelegate nextMiddleware;
    private readonly SecureResponseHeadersOptions secureResponseHeadersOptions;

    public SecureResponseHeadersMiddleware(RequestDelegate nextMiddleware, IOptions<SecureResponseHeadersOptions> optionsAccessor)
        => (this.nextMiddleware, secureResponseHeadersOptions) = (nextMiddleware, optionsAccessor.Value);

    public Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.OnStarting(
            static httpContextAsObject =>
            {
                var (httpContext, trustedTypesPolicies) = ((HttpContext, IList<string>))httpContextAsObject;
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
                {
                    cspBuilder.Append(CultureInfo.InvariantCulture, $"'strict-dynamic' 'nonce-{nonceValue}' 'unsafe-inline' https:");

                    if (trustedTypesPolicies.Count > 0)
                        cspBuilder.Append(CultureInfo.InvariantCulture, $"; trusted-types '{string.Join("', '", trustedTypesPolicies.ToHashSet())}'");
                }
                else
                {
                    cspBuilder.Append("'none'");
                }

                cspBuilder.Append("; require-trusted-types-for 'script'");

                httpResponse.Headers["Content-Security-Policy"] = cspBuilder.ToString();

                // https://github.com/w3c/webappsec-permissions-policy/blob/main/features.md
                httpResponseHeaders["Permissions-Policy"] =
                    "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), bluetooth=(), camera=(), ch-ua=(), ch-ua-arch=(), ch-ua-bitness=(), ch-ua-full-version=(), ch-ua-full-version-list=(), ch-ua-mobile=(), ch-ua-model=(), ch-ua-platform=(), ch-ua-platform-version=(), ch-ua-wow64=(), cross-origin-isolated=(), display-capture=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), hid=(), idle-detection=(), keyboard-map=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), serial=(), sync-xhr=(), usb=(), web-share=()";

                httpResponseHeaders["Referrer-Policy"] = "strict-origin-when-cross-origin";

                return Task.CompletedTask;
            },
            (httpContext, secureResponseHeadersOptions.TrustedTypesPolicies));

        return nextMiddleware(httpContext);
    }
}
