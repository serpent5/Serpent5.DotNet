using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class CookiePolicyOptionsSetupTests
{
    [Fact]
    public void Requires_Secure_Cookies()
    {
        var cookiePolicyOptionsSetup = new CookiePolicyOptionsSetup();
        var cookiePolicyOptions = new CookiePolicyOptions();

        cookiePolicyOptionsSetup.Configure(cookiePolicyOptions);

        Assert.Equal(CookieSecurePolicy.Always, cookiePolicyOptions.Secure);
    }
}
