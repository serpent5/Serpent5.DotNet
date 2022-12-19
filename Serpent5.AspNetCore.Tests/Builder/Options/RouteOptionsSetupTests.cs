using Microsoft.AspNetCore.Routing;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class RouteOptionsSetupTests
{
    [Fact]
    public void EnablesLowercaseUrls()
    {
        var routeOptionsSetup = new RouteOptionsSetup();
        var routeOptions = new RouteOptions();

        routeOptionsSetup.Configure(routeOptions);

        Assert.True(routeOptions.LowercaseUrls);
    }
}
