using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.Middleware;

public class CacheResponseHeadersMiddlewareTests
{
    private static readonly DateTimeOffset anyDateTimeOffset = DateTimeOffset.MinValue;
    private const string anyETag = "*";

    [Fact]
    public async Task SetsCacheControlToNoStore()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync();

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlIsSet_DoesNotSetCacheControlToNoStore()
    {
        const string anyUnaffectedCacheControlValue = "public, no-cache";

        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.CacheControl = anyUnaffectedCacheControlValue);

        Assert.Equal(anyUnaffectedCacheControlValue, httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlIsSetToNoCacheNoStore_SetsCacheControlToNoStore()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.CacheControl = "no-cache, no-store");

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task CacheControlIsImmutable_RemovesETag()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.ETag = anyETag;
            ctx.Response.Headers.CacheControl = "immutable";
        });

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.ETag));
    }

    [Fact]
    public async Task CacheControlIsImmutable_RemovesLastModified()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.GetTypedHeaders().LastModified = anyDateTimeOffset;
            ctx.Response.Headers.CacheControl = "immutable";
        });

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.LastModified));
    }

    [Fact]
    public async Task ETagIsSet_RemovesLastModified()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.ETag = anyETag;
            ctx.Response.Headers.LastModified = anyDateTimeOffset.ToString("R");
        });

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.LastModified));
    }

    [Fact]
    public async Task ETagIsSet_SetsCacheControlToNoCache()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.ETag = anyETag);

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task LastModifiedIsSet_SetsCacheControlToNoCache()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.LastModified = anyDateTimeOffset.ToString("R"));

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task RemovesExpires()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.Expires = anyDateTimeOffset.ToString("R"));

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.Expires));
    }

    [Fact]
    public async Task RemovesPragma()
    {
        const string onlyValidPragmaValue = "no-cache";

        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.Pragma = onlyValidPragmaValue);

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.Pragma));
    }

    private static async ValueTask<HttpResponseMessage> RunMiddlewarePipelineAsync(Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await TestHostBuilder.StartAsync(applicationBuilder =>
        {
            applicationBuilder.UseCacheResponseHeaders();

            if (configureHttpContext is null)
                return;

            applicationBuilder.Run(ctx =>
            {
                configureHttpContext(ctx);
                return Task.CompletedTask;
            });
        });

        return await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
    }
}
