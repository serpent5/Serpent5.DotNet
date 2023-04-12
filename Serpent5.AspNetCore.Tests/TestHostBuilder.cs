using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Serpent5.AspNetCore.Tests;

internal sealed class TestHostBuilder
{
    private readonly List<Action<IServiceCollection>> configureServicesDelegates = new();
    private readonly List<Action<IApplicationBuilder>> configureDelegates = new();

    public TestHostBuilder ConfigureServices(Action<IServiceCollection> configureServicesDelegate)
    {
        configureServicesDelegates.Add(configureServicesDelegate);
        return this;
    }

    public TestHostBuilder Configure(Action<IApplicationBuilder> configureDelegate)
    {
        configureDelegates.Add(configureDelegate);
        return this;
    }

    public Task<IHost> StartAsync()
    {
        var hostBuilder = new HostBuilder();

        configureServicesDelegates.ForEach(x => hostBuilder.ConfigureServices(x));

        return hostBuilder
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseTestServer();
                configureDelegates.ForEach(x => webHostBuilder.Configure(x));
            })
            .StartAsync();
    }
}
