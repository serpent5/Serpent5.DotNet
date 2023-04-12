using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.Middleware;

using static MediaTypeNames;

public class SecureResponseHeadersMiddlewareExtensionsTests
{
    private const string htmlContentType = Text.Html;
    private const string anyContentTypeExceptHtml = Text.Plain;

    [Theory]
    [InlineData(htmlContentType)]
    [InlineData(anyContentTypeExceptHtml)]
    public async Task Sets_XContentTypeOptions_To_NoSniff(string contentType)
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(contentType);

        Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.XContentTypeOptions, out var headerValues));
        Assert.Equal("nosniff", Assert.Single(headerValues));
    }

    // ReSharper disable once InconsistentNaming
    public class Response_Content_Is_HTML
    {
        [Fact]
        public async Task Sets_ContentSecurityPolicy_To_Google_Recommended_Strict()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

            Assert.True(httpResponseMessage.Headers.TryGetValues(HeaderNames.ContentSecurityPolicy, out var headerValues));

            var singleHeaderValue = Assert.Single(headerValues);
            var cspDirectives = singleHeaderValue.Split(";", StringSplitOptions.TrimEntries);

            Assert.Contains("base-uri 'self'", cspDirectives);
            Assert.Contains("frame-ancestors 'none'", cspDirectives);
            Assert.Contains("object-src 'none'", cspDirectives);
            Assert.Contains("script-src 'none'", cspDirectives);
            Assert.Contains("require-trusted-types-for 'script'", cspDirectives);
        }

        [Fact]
        public async Task Sets_ContentSecurityPolicy_To_Google_Recommended_Strict_With_Script_Nonce_When_Script_Nonce_Exists()
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
        public async Task Sets_PermissionsPolicy()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

            Assert.True(httpResponseMessage.Headers.Contains("Permissions-Policy"));
        }

        [Fact]
        public async Task Sets_ReferrerPolicy_To_StrictOriginWhenCrossOrigin()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(htmlContentType);

            Assert.True(httpResponseMessage.Headers.TryGetValues("Referrer-Policy", out var headerValues));
            Assert.Equal("strict-origin-when-cross-origin", Assert.Single(headerValues));
        }
    }

    // ReSharper disable once InconsistentNaming
    public class Response_Content_Is_Not_HTML
    {
        [Fact]
        public async Task Does_Not_Set_ContentSecurityPolicy()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

            Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.ContentSecurityPolicy));
        }

        [Fact]
        public async Task Does_Not_Set_ReferrerPolicy()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

            Assert.False(httpResponseMessage.Headers.Contains("Referrer-Policy"));
        }

        [Fact]
        public async Task Does_Not_Set_PermissionsPolicy()
        {
            var httpResponseMessage = await RunMiddlewarePipelineAsync(anyContentTypeExceptHtml);

            Assert.False(httpResponseMessage.Headers.Contains("Permissions-Policy"));
        }
    }

    private static async ValueTask<HttpResponseMessage> RunMiddlewarePipelineAsync(string contentType, Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await new TestHostBuilder()
            .Configure(applicationBuilder =>
            {
                applicationBuilder.UseSecureResponseHeaders();

                applicationBuilder.Run(ctx =>
                {
                    ctx.Response.ContentType = contentType;
                    configureHttpContext?.Invoke(ctx);
                    return Task.CompletedTask;
                });
            })
            .StartAsync();

        return await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
    }
}
