using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.Middleware;

public class CacheResponseHeadersMiddlewareTests
{
    private readonly DateTimeOffset anyDateTimeOffset = DateTimeOffset.Now;

    [Fact]
    public async Task SetsCacheControlValueToNoStore()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync();

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task DoesNotChangeExistingCacheControlValue()
    {
        const string anyValidCacheControlValueExceptNoStore = "public, no-cache";

        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.CacheControl = anyValidCacheControlValueExceptNoStore;
        });

        Assert.Equal(anyValidCacheControlValueExceptNoStore, httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task ChangesCacheControlValueToNoStoreWhenSetToNoCacheNoStore()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.CacheControl = "no-cache, no-store";
        });

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task RemovesETagWhenCacheControlIsImmutable()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(ctx =>
        {
            ctx.Response.Headers.CacheControl = "immutable";

            var httpResponseHeaders = ctx.Response.GetTypedHeaders();

            httpResponseHeaders.ETag = EntityTagHeaderValue.Any;
            httpResponseHeaders.LastModified = anyDateTimeOffset;
        });

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.ETag));
    }

    [Fact]
    public async Task RemovesLastModifiedWhenETagIsSet()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(ctx =>
        {
            var httpResponseHeaders = ctx.Response.GetTypedHeaders();

            httpResponseHeaders.ETag = EntityTagHeaderValue.Any;
            httpResponseHeaders.LastModified = anyDateTimeOffset;
        });

        Assert.Null(httpResponseMessage.Content.Headers.LastModified);
    }

    [Fact]
    public async Task DoesNotRemoveLastModifiedWhenTagIsNotSet()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(ctx =>
        {
            ctx.Response.GetTypedHeaders().LastModified = anyDateTimeOffset;
        });

        Assert.NotNull(httpResponseMessage.Content.Headers.LastModified);
    }

    [Fact]
    public async Task RemovesExpires()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(ctx =>
        {
            ctx.Response.GetTypedHeaders().Expires = anyDateTimeOffset;
        });

        Assert.Null(httpResponseMessage.Content.Headers.Expires);
    }

    [Fact]
    public async Task RemovesPragma()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.Pragma = "no-cache";
        });

        Assert.Empty(httpResponseMessage.Headers.Pragma);
    }

    [Fact]
    public async Task SetsCacheControlValueToNoCacheWhenETagIsSet()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.GetTypedHeaders().ETag = EntityTagHeaderValue.Any;
        });

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task SetsCacheControlValueToNoCacheWhenLastModifiedIsSet()
    {
        using var httpResponseMessage = await RunCacheHeadersMiddlewarePipelineAsync(ctx =>
        {
            ctx.Response.GetTypedHeaders().LastModified = anyDateTimeOffset;
        });

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    private static async ValueTask<HttpResponseMessage> RunCacheHeadersMiddlewarePipelineAsync(Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await TestHostBuilder.StartAsync(applicationBuilder =>
        {
            applicationBuilder.UseCacheResponseHeaders();

            if (configureHttpContext is null)
                return;

            applicationBuilder.Use(async (ctx, nextMiddleware) =>
            {
                configureHttpContext(ctx);
                await nextMiddleware(ctx);
            });
        });

        return await testHost.GetTestClient().GetAsync("/");
    }
}
