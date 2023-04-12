using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class HttpsRedirectionOptionsSetupTests
{
    [Fact]
    public void Configures_Permanent_Redirect()
    {
        var httpsRedirectionOptionsSetup = new HttpsRedirectionOptionsSetup();
        var httpsRedirectionOptions = new HttpsRedirectionOptions();

        httpsRedirectionOptionsSetup.Configure(httpsRedirectionOptions);

        Assert.Equal(StatusCodes.Status301MovedPermanently, httpsRedirectionOptions.RedirectStatusCode);
    }
}
