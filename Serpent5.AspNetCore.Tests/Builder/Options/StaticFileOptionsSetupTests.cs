using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Net.Http.Headers;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class StaticFileOptionsSetupTests
{
    [Theory]
    [InlineData(".css")]
    [InlineData(".js")]
    [InlineData(".txt")]
    [InlineData(".xml")]
    // ReSharper disable once InconsistentNaming
    public void SetsEncodingToUTF8(string contentTypeExtension)
    {
        var staticFileOptionsSetup = new StaticFileOptionsSetup();
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsSetup.Configure(staticFileOptions);

        Assert.True(staticFileOptions.ContentTypeProvider.TryGetContentType(contentTypeExtension, out var contentType));
        Assert.True(MediaTypeHeaderValue.TryParse(contentType, out var mediaTypeHeaderValue));
        Assert.Equal(Encoding.UTF8, mediaTypeHeaderValue.Encoding);
    }

    [Fact]
    // ReSharper disable once InconsistentNaming
    public void DoesNotServeHTML()
    {
        var staticFileOptionsSetup = new StaticFileOptionsSetup();
        var staticFileOptions = new StaticFileOptions();

        staticFileOptionsSetup.Configure(staticFileOptions);

        Assert.False(staticFileOptions.ContentTypeProvider.TryGetContentType(".html", out _));
    }
}
