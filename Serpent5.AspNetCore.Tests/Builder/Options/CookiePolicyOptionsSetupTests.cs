using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class CookiePolicyOptionsSetupTests
{
    [Fact]
    public void RequiresSecureCookies()
    {
        var cookiePolicyOptionsSetup = new CookiePolicyOptionsSetup();
        var cookiePolicyOptions = new CookiePolicyOptions();

        cookiePolicyOptionsSetup.Configure(cookiePolicyOptions);

        Assert.Equal(CookieSecurePolicy.Always, cookiePolicyOptions.Secure);
    }
}
