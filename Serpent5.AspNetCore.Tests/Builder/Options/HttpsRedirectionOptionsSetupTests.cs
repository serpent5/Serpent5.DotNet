using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class HttpsRedirectionOptionsSetupTests
{
    [Fact]
    public void ConfiguresPermanentRedirect()
    {
        var httpsRedirectionOptionsSetup = new HttpsRedirectionOptionsSetup();
        var httpsRedirectionOptions = new HttpsRedirectionOptions();

        httpsRedirectionOptionsSetup.Configure(httpsRedirectionOptions);

        Assert.Equal(StatusCodes.Status301MovedPermanently, httpsRedirectionOptions.RedirectStatusCode);
    }
}
