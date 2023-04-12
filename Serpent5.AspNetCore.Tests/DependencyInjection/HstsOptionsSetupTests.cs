using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class HstsOptionsSetupTests
{
    [Fact]
    public void Configures_Preload_Deployment_Recommendations() // https://hstspreload.org/#deployment-recommendations
    {
        var hstsOptionsSetup = new HstsOptionsSetup();
        var hstsOptions = new HstsOptions();

        hstsOptionsSetup.Configure(hstsOptions);

        Assert.Equal(hstsOptions.MaxAge, TimeSpan.FromSeconds(63072000)); // 2 Years.
        Assert.True(hstsOptions.Preload);
        Assert.True(hstsOptions.IncludeSubDomains);
    }
}
