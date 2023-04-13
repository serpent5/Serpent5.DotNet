using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal abstract class WebApplicationBehavior
{
    public abstract void Configure(WebApplicationBuilder webApplicationBuilder);

    protected static void ConfigureOptions<TOptions, TOptionsSetup>(WebApplicationBuilder webApplicationBuilder)
        where TOptions : class
        where TOptionsSetup : class, IConfigureOptions<TOptions>
        => webApplicationBuilder.Services.AddTransient<IConfigureOptions<TOptions>, TOptionsSetup>();
}
