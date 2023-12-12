using System.Globalization;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using static System.Net.Mime.MediaTypeNames;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal sealed class SecureResponseHeadersMiddleware(RequestDelegate nextMiddleware, IOptions<SecureResponseHeadersOptions> optionsAccessor)
{
    private static readonly MediaTypeHeaderValue htmlMediaTypeHeaderValue = new(Text.Html);

    private readonly SecureResponseHeadersOptions secureResponseHeadersOptions = optionsAccessor.Value;

    [UsedImplicitly]
    public Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Features.Set(new SecureResponseHeadersFeature());

        httpContext.Response.OnStarting(
            static callbackState =>
            {
                var (httpContext, secureResponseHeadersOptions) = ((HttpContext, SecureResponseHeadersOptions))callbackState;
                var httpResponse = httpContext.Response;
                var httpResponseHeaders = httpResponse.Headers;

                httpResponseHeaders.XContentTypeOptions = "nosniff";

                if (httpResponse.GetTypedHeaders().ContentType?.IsSubsetOf(htmlMediaTypeHeaderValue) != true)
                    return Task.CompletedTask;

                httpResponseHeaders.ContentSecurityPolicy = GetContentSecurityPolicy(httpContext, secureResponseHeadersOptions);
                httpResponseHeaders["Permissions-Policy"] = GetPermissionsPolicy();
                httpResponseHeaders["Referrer-Policy"] = "strict-origin-when-cross-origin";

                return Task.CompletedTask;
            },
            (httpContext, secureResponseHeadersOptions));

        return nextMiddleware(httpContext);
    }

    private static string GetContentSecurityPolicy(HttpContext httpContext, SecureResponseHeadersOptions secureResponseHeadersOptions)
    {
        // https://web.dev/strict-csp
        // https://csp.withgoogle.com/docs/strict-csp.html
        var cspBuilder = new StringBuilder("base-uri 'self'; frame-ancestors 'none'; object-src 'none'; ");
        var cultureInfo = CultureInfo.InvariantCulture;

        cspBuilder.Append("script-src ");

        if (httpContext.TryGetNonce(out var nonceValue))
        {
            cspBuilder.Append(cultureInfo, $"'strict-dynamic' 'nonce-{nonceValue}' 'unsafe-inline'");

            if (secureResponseHeadersOptions.UnsafeEval)
                cspBuilder.Append(" 'unsafe-eval'");

            cspBuilder.Append(" https:");
        }
        else
        {
            cspBuilder.Append("'none'");
        }

        if (httpContext.Features.Get<SecureResponseHeadersFeature>()?.RequireTrustedTypes != false)
        {
            var trustedTypesPolicies = secureResponseHeadersOptions.TrustedTypesPolicies;
            var cspTrustedTypesPolicies = nonceValue is not null && trustedTypesPolicies.Count > 0
                ? string.Join(' ', trustedTypesPolicies)
                : "'none'";

            cspBuilder.Append(cultureInfo, $"; trusted-types {cspTrustedTypesPolicies}; ");
            cspBuilder.Append("require-trusted-types-for 'script'");
        }

        return cspBuilder.ToString();
    }

    private static string GetPermissionsPolicy()
    {
        // https://github.com/w3c/webappsec-permissions-policy/blob/main/features.md
        var policyControlledFeatures = new[]
        {
            "accelerometer",
            "ambient-light-sensor",
            "autoplay",
            "battery",
            "bluetooth",
            "camera",
            "ch-ua",
            "ch-ua-arch",
            "ch-ua-bitness",
            "ch-ua-full-version",
            "ch-ua-full-version-list",
            "ch-ua-mobile",
            "ch-ua-model",
            "ch-ua-platform",
            "ch-ua-platform-version",
            "ch-ua-wow64",
            "cross-origin-isolated",
            "display-capture",
            "encrypted-media",
            "execution-while-not-rendered",
            "execution-while-out-of-viewport",
            "fullscreen",
            "geolocation",
            "gyroscope",
            "hid",
            "idle-detection",
            "keyboard-map",
            "magnetometer",
            "microphone",
            "midi",
            "navigation-override",
            "payment",
            "picture-in-picture",
            "publickey-credentials-get",
            "screen-wake-lock",
            "serial",
            "sync-xhr",
            "usb",
            "web-share"
        };

        return string.Join(
            ", ",
            policyControlledFeatures.Select(static x => string.Concat(x, "=", "()")));
    }
}
