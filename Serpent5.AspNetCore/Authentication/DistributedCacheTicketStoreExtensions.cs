using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Serpent5.AspNetCore.Authentication;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extension methods for using <see cref="ITicketStore" /> with <see cref="IDistributedCache" />.
/// </summary>
[PublicAPI]
public static class DistributedCacheTicketStoreExtensions
{
    /// <summary>
    /// Adds an implementation of <see cref="ITicketStore" /> that uses <see cref="IDistributedCache" /> as a backing-store for the <see cref="CookieAuthenticationDefaults.AuthenticationScheme" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <returns>A reference to the provided <see cref="IServiceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddCookiesDistributedCacheTicketStore(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddTransient<DistributedCacheTicketStore>();

        serviceCollection.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<DistributedCacheTicketStore, IOptionsMonitor<DistributedCacheTicketStoreOptions>>(static (o, distributedCacheTicketStore, optionsMonitor) =>
            {
                o.SessionStore = distributedCacheTicketStore;
                o.ExpireTimeSpan = optionsMonitor.CurrentValue.Expires;
                o.SlidingExpiration = false;
            });

        return serviceCollection;
    }

    /// <summary>
    /// Adds an implementation of <see cref="ITicketStore" /> that uses <see cref="IDistributedCache" /> as a backing-store for the <see cref="CookieAuthenticationDefaults.AuthenticationScheme" />.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="setupAction">An <see cref="Action{T}"/> to configure <see cref="DistributedCacheTicketStoreOptions" />.</param>
    /// <returns>A reference to the provided <see cref="IServiceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddCookiesDistributedCacheTicketStore(this IServiceCollection serviceCollection, Action<DistributedCacheTicketStoreOptions> setupAction)
    {
        serviceCollection.AddCookiesDistributedCacheTicketStore();
        serviceCollection.Configure(setupAction);

        return serviceCollection;
    }
}
