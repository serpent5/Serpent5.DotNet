using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Serpent5.AspNetCore.Tests;

internal static class TestHostBuilder
{
    public static Task<IHost> StartAsync(Action<IServiceCollection>? configureServices, Action<IApplicationBuilder> configure)
    {
        var hostBuilder = new HostBuilder();

        if (configureServices is not null)
            hostBuilder.ConfigureServices(configureServices);

        return hostBuilder
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseTestServer()
                    .Configure(configure);
            })
            .StartAsync();
    }

    public static Task<IHost> StartAsync(Action<IApplicationBuilder> configure)
        => StartAsync(_ => { }, configure);
};
