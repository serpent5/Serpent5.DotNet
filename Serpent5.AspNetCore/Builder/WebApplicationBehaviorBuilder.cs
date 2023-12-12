using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class WebApplicationBehaviorBuilder : IWebApplicationBehaviorBuilder
{
    private readonly List<WebApplicationBehavior> webApplicationBehaviors;
    private readonly List<Action<IServiceCollection>> configureServicesDelegates = [];

    public WebApplicationBehaviorBuilder(string appName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appName);

        webApplicationBehaviors = [
            new DefaultWebApplicationBehavior(appName)
        ];
    }

    public IWebApplicationBehaviorBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        configureServicesDelegates.Add(configureServices);
        return this;
    }

    public IWebApplicationBehaviorBuilder AddBehavior(WebApplicationBehavior webApplicationBehavior)
    {
        webApplicationBehaviors.Add(webApplicationBehavior);
        return this;
    }

    internal void Configure(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBehaviors.ForEach(webApplicationBehavior => webApplicationBehavior.Configure(webApplicationBuilder));
        configureServicesDelegates.ForEach(configureServices => configureServices(webApplicationBuilder.Services));
    }
}
