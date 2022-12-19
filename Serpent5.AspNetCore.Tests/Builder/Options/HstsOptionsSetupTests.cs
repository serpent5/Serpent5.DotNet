using Microsoft.AspNetCore.HttpsPolicy;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class HstsOptionsSetupTests
{
    [Fact]
    public void ConfiguresPreloadDeploymentRecommendations() // https://hstspreload.org/#deployment-recommendations
    {
        var hstsOptionsSetup = new HstsOptionsSetup();
        var hstsOptions = new HstsOptions();

        hstsOptionsSetup.Configure(hstsOptions);

        Assert.Equal(hstsOptions.MaxAge, TimeSpan.FromSeconds(63072000)); // 2 Years.
        Assert.True(hstsOptions.Preload);
        Assert.True(hstsOptions.IncludeSubDomains);
    }
}
