using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Serpent5.AspNetCore.Tests;

internal static class TestHostBuilder
{
    public static Task<IHost> StartAsync(Action<IApplicationBuilder> configure)
    {
        var hostBuilder = new HostBuilder();

        return hostBuilder
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseTestServer()
                    .Configure(configure);
            })
            .StartAsync();
    }
}
