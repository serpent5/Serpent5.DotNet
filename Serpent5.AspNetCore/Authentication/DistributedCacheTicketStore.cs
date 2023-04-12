using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Authentication;

internal sealed class DistributedCacheTicketStore : ITicketStore
{
    private readonly IDistributedCache distributedCache;
    private readonly IOptionsMonitor<DistributedCacheTicketStoreOptions> optionsMonitor;

    public DistributedCacheTicketStore(IDistributedCache distributedCache, IOptionsMonitor<DistributedCacheTicketStoreOptions> optionsMonitor)
        => (this.distributedCache, this.optionsMonitor) = (distributedCache, optionsMonitor);

    public Task<string> StoreAsync(AuthenticationTicket authenticationTicket)
    {
        Span<byte> randomBytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(randomBytes);

        var sessionKey = Convert.ToBase64String(randomBytes);

        return StoreAsyncInternal();

        async Task<string> StoreAsyncInternal()
        {
            await CacheAsync(sessionKey, authenticationTicket).ConfigureAwait(false);
            return sessionKey;
        }
    }

    public Task RenewAsync(string sessionKey, AuthenticationTicket authenticationTicket)
    {
        ArgumentNullException.ThrowIfNull(authenticationTicket);

        return CacheAsync(sessionKey, authenticationTicket);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string sessionKey)
    {
        var authenticationTicketAsBytes = await distributedCache.GetAsync(sessionKey).ConfigureAwait(false);

        return authenticationTicketAsBytes is null
            ? null
            : TicketSerializer.Default.Deserialize(authenticationTicketAsBytes);
    }

    public Task RemoveAsync(string sessionKey)
        => distributedCache.RemoveAsync(sessionKey);

    private Task CacheAsync(string sessionKey, AuthenticationTicket authenticationTicket)
        => distributedCache.SetAsync(
            sessionKey,
            TicketSerializer.Default.Serialize(authenticationTicket),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = authenticationTicket.Properties.ExpiresUtc,
                SlidingExpiration = optionsMonitor.CurrentValue.SlidingExpiration
            });
}
