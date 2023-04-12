using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class StaticFileOptionsSetupTests
{
    [Theory]
    [InlineData(".css")]
    [InlineData(".js")]
    [InlineData(".svg")]
    [InlineData(".txt")]
    [InlineData(".xml")]
    public void Sets_Content_Encoding_To_UTF8(string contentTypeExtension)
    {
        var staticFileOptionsSetup = new StaticFileOptionsSetup(TestFakes.HostEnvironment());
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsSetup.Configure(staticFileOptions);

        Assert.True(staticFileOptions.ContentTypeProvider.TryGetContentType(contentTypeExtension, out var contentType));
        Assert.True(MediaTypeHeaderValue.TryParse(contentType, out var mediaTypeHeaderValue));
        Assert.Equal(Encoding.UTF8, mediaTypeHeaderValue.Encoding);
    }

    [Fact]
    public void Does_Not_Support_HTML()
    {
        var staticFileOptionsSetup = new StaticFileOptionsSetup(TestFakes.HostEnvironment());
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsSetup.Configure(staticFileOptions);

        Assert.False(staticFileOptions.ContentTypeProvider.TryGetContentType(".html", out _));
    }

    [Fact]
    public void Sets_Cache_Control_To_NoCache_When_Request_File_Is_Mutable()
    {
        var staticFileOptionsProductionSetup = new StaticFileOptionsSetup(TestFakes.HostEnvironment());
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = TestFakes.HttpContext();

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, TestFakes.FileInfo(string.Empty)));

        Assert.Equal("no-cache", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }

    [Fact]
    public void Sets_Cache_Control_To_Immutable_With_One_Year_Expiration_When_Request_Query_Includes_Version_Identifier()
    {
        var staticFileOptionsProductionSetup = new StaticFileOptionsSetup(TestFakes.HostEnvironment());
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = TestFakes.HttpContext();

        fakeHttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["v"] = TestFakes.String
        });

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, TestFakes.FileInfo()));

        Assert.Equal("max-age=31536000, immutable", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }

    [Fact]
    public void Sets_Cache_Control_To_Immutable_With_One_Year_Expiration_When_Request_File_Name_Is_Angular_Immutable_File()
    {
        const string anyAngularImmutableFileName = "any.abcdef1234567890.any";

        var staticFileOptionsProductionSetup = new StaticFileOptionsSetup(TestFakes.HostEnvironment());
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = TestFakes.HttpContext();

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, TestFakes.FileInfo(anyAngularImmutableFileName)));

        Assert.Equal("max-age=31536000, immutable", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }
}
