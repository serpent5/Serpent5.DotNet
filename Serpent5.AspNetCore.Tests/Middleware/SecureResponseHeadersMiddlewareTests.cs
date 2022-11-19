using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.Middleware;

public class SecureResponseHeadersMiddlewareTests
{
    private const string htmlContentType = "text/html";
    private const string anyContentTypeExceptHtml = "text/plain";

    [Theory]
    [InlineData(htmlContentType)]
    [InlineData(anyContentTypeExceptHtml)]
    public async Task SetsXContentTypeOptionsToNoSniff(string contentType)
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(contentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.XContentTypeOptions, out var headerValues));
        Assert.Equal("nosniff", Assert.Single(headerValues));
    }

    [Fact]
    public async Task SetsContentSecurityPolicy()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.ContentSecurityPolicy, out var headerValues));

        var singleHeaderValue = Assert.Single(headerValues);
        var cspDirectives = singleHeaderValue.Split(";", StringSplitOptions.TrimEntries);

        Assert.Contains("base-uri 'self'", cspDirectives);
        Assert.Contains("frame-ancestors 'none'", cspDirectives);
        Assert.Contains("object-src 'none'", cspDirectives);
        Assert.Contains("require-trusted-types-for 'script'", cspDirectives);
    }

    [Fact]
    public async Task SetsContentSecurityPolicyScriptNone()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.ContentSecurityPolicy, out var headerValues));
        var singleHeaderValue = Assert.Single(headerValues);
        var cspDirectives = singleHeaderValue.Split(";", StringSplitOptions.TrimEntries);

        Assert.Equal(
            "script-src 'none'",
            Assert.Single(cspDirectives, x => x.StartsWith("script-src", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task SetsContentSecurityPolicyScriptWithNonce()
    {
        var nonceValue = string.Empty;
        var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType, ctx => nonceValue = ctx.GetNonce());

        Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.ContentSecurityPolicy, out var headerValues));

        var singleHeaderValue = Assert.Single(headerValues);
        var cspDirectives = singleHeaderValue.Split(";", StringSplitOptions.TrimEntries);

        Assert.Equal(
            $"script-src 'strict-dynamic' 'nonce-{nonceValue}' 'unsafe-inline' https:",
            Assert.Single(cspDirectives, x => x.StartsWith("script-src", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task SetsPermissionsPolicy()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues("Permissions-Policy", out var headerValues));
        Assert.Equal(
            "accelerometer=(), ambient-light-sensor=(), autoplay=(), battery=(), camera=(), cross-origin-isolated=(), display-capture=(), document-domain=(), encrypted-media=(), execution-while-not-rendered=(), execution-while-out-of-viewport=(), fullscreen=(), geolocation=(), gyroscope=(), hid=(), idle-detection=(), magnetometer=(), microphone=(), midi=(), navigation-override=(), payment=(), picture-in-picture=(), publickey-credentials-get=(), screen-wake-lock=(), serial=(), sync-xhr=(), usb=(), web-share=(), xr-spatial-tracking=()",
            Assert.Single(headerValues));
    }

    [Fact]
    public async Task SetsReferrerPolicyToStrictOriginWhenCrossOrigin()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues("Referrer-Policy", out var headerValues));
        Assert.Equal("strict-origin-when-cross-origin", Assert.Single(headerValues));
    }

    [Fact]
    public async Task DoesNotSetContentSecurityPolicy()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.ContentSecurityPolicy));
    }

    [Fact]
    public async Task DoesNotSetReferrerPolicy()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

        Assert.False(httpResponseMessage.Headers.Contains("Referrer-Policy"));
    }

    [Fact]
    public async Task DoesNotSetPermissionsPolicy()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

        Assert.False(httpResponseMessage.Headers.Contains("Permissions-Policy"));
    }

    private static async ValueTask<HttpResponseMessage> RunMiddlewarePipelineAsync(string contentType, Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await TestHostBuilder.StartAsync(applicationBuilder =>
        {
            applicationBuilder.UseSecureResponseHeaders();

            applicationBuilder.Run(ctx =>
            {
                ctx.Response.ContentType = contentType;
                configureHttpContext?.Invoke(ctx);
                return Task.CompletedTask;
            });
        });

        return await testHost.GetTestClient().GetAsync("/");
    }
}
