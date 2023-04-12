using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.Middleware;

public class CacheResponseHeadersMiddlewareExtensionsTests
{
    private const string anyETag = "*";

    [Fact]
    public async Task Sets_CacheControl_To_NoStore()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync();

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Does_Not_Change_CacheControl_When_Set()
    {
        const string anyUnaffectedCacheControlValue = "public, no-cache";

        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.CacheControl = anyUnaffectedCacheControlValue);

        Assert.Equal(anyUnaffectedCacheControlValue, httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Removes_NoCache_From_CacheControl_When_Set_To_NoCache_NoStore()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.CacheControl = "no-cache, no-store");

        Assert.Equal("no-store", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Removes_ETag_When_CacheControl_Is_Set_To_Immutable()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.ETag = anyETag;
            ctx.Response.Headers.CacheControl = "immutable";
        });

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.ETag));
    }

    [Fact]
    public async Task Removes_LastModified_When_CacheControl_Is_Set_To_Immutable()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.GetTypedHeaders().LastModified = TestFakes.DateTimeOffset();
            ctx.Response.Headers.CacheControl = "immutable";
        });

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.LastModified));
    }

    [Fact]
    public async Task Removes_LastModified_When_ETag_Is_Set()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(static ctx =>
        {
            ctx.Response.Headers.ETag = anyETag;
            ctx.Response.Headers.LastModified = TestFakes.DateTimeOffset().ToString("R");
        });

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.LastModified));
    }

    [Fact]
    public async Task Sets_CacheControl_To_NoCache_When_ETag_Is_Set()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.ETag = anyETag);

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Sets_CacheControl_To_NoCache_When_LastModified_Is_Set()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.LastModified = TestFakes.DateTimeOffset().ToString("R"));

        Assert.Equal("no-cache", httpResponseMessage.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Removes_Expires()
    {
        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.Expires = TestFakes.DateTimeOffset().ToString("R"));

        Assert.False(httpResponseMessage.Content.Headers.Contains(HeaderNames.Expires));
    }

    [Fact]
    public async Task Removes_Pragma()
    {
        const string onlyValidPragmaValue = "no-cache";

        var httpResponseMessage = await RunMiddlewarePipelineAsync(
            static ctx => ctx.Response.Headers.Pragma = onlyValidPragmaValue);

        Assert.False(httpResponseMessage.Headers.Contains(HeaderNames.Pragma));
    }

    private static async ValueTask<HttpResponseMessage> RunMiddlewarePipelineAsync(Action<HttpContext>? configureHttpContext = null)
    {
        var testHost = await new TestHostBuilder()
            .Configure(applicationBuilder =>
            {
                applicationBuilder.UseCacheResponseHeaders();

                if (configureHttpContext is null)
                    return;

                applicationBuilder.Run(ctx =>
                {
                    configureHttpContext(ctx);
                    return Task.CompletedTask;
                });
            })
            .StartAsync();

        return await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));
    }
}
