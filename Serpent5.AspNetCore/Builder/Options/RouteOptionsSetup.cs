using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class RouteOptionsSetup : IConfigureOptions<RouteOptions>
{
    public void Configure(RouteOptions routeOptions)
        => routeOptions.LowercaseUrls = true;
}
