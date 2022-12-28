using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Moq;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class StaticFileOptionsProductionSetupTests
{
    [Fact]
    public void RequestFileIsMutable_SetsCacheControlNoCache()
    {
        var staticFileOptionsProductionSetup = new StaticFileOptionsProductionSetup();
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = CreateFakeHttpContext();
        var fakeFileInfo = Mock.Of<IFileInfo>(x => x.Name == string.Empty);

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, fakeFileInfo));

        Assert.Equal("no-cache", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }

    [Fact]
    public void RequestQueryIncludesVersion_SetsCacheControlOneYearImmutable()
    {
        var staticFileOptionsProductionSetup = new StaticFileOptionsProductionSetup();
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = CreateFakeHttpContext();
        fakeHttpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
        {
            ["v"] = "anyValue"
        });

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, Mock.Of<IFileInfo>()));

        Assert.Equal("max-age=31536000, immutable", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }

    [Fact]
    public void RequestFileNameIsAngularImmutableFile_SetsCacheControlOneYearImmutable()
    {
        const string anyAngularImmutableFileName = "any.abcdef1234567890.any";

        var staticFileOptionsProductionSetup = new StaticFileOptionsProductionSetup();
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsProductionSetup.Configure(staticFileOptions);

        var fakeHttpContext = CreateFakeHttpContext();
        var fakeFileInfo = Mock.Of<IFileInfo>(x => x.Name == anyAngularImmutableFileName);

        staticFileOptions.OnPrepareResponse(new StaticFileResponseContext(fakeHttpContext, fakeFileInfo));

        Assert.Equal("max-age=31536000, immutable", fakeHttpContext.Response.GetTypedHeaders().CacheControl?.ToString());
    }

    private static HttpContext CreateFakeHttpContext()
    {
        var fakeHttpContext = TestFakes.HttpContext();
        fakeHttpContext.Request.Query = new QueryCollection();
        return fakeHttpContext;
    }
}
