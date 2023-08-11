using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Tests.Authentication;

public class DistributedCacheTicketStoreTests
{
    private const string fakeAuthenticationScheme = TestFakes.String;
    private const string fakeSessionKey = TestFakes.String;

    private static readonly TimeSpan fakeExpires = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan fakeSlidingExpiration = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task StoreAsync_Creates_A_Session()
    {
        const int expectedSessionKeyByteCount = 32;

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore();
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket();

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        Assert.NotEmpty(sessionKey);
        Assert.Equal(expectedSessionKeyByteCount, Convert.FromBase64String(sessionKey).Length);
    }

    [Fact]
    public async Task StoreAsync_Stores_Value_In_DistributedCache()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions();

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(fakeDistributedCacheTicketStoreOptions, fakeDistributedCache);

        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket();

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(fakeDistributedCache, sessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task StoreAsync_Stores_Value_In_DistributedCache_With_Specified_Options()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions
        {
            Expires = fakeExpires,
            SlidingExpiration = fakeSlidingExpiration
        };

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(fakeDistributedCacheTicketStoreOptions, fakeDistributedCache);
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket();

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(fakeDistributedCache, sessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RetrieveAsync_Retrieves_AuthenticationTicket_From_Cache()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();

        A.CallTo(() => fakeDistributedCache.GetAsync(fakeSessionKey, A<CancellationToken>._))
            .Returns((byte[]?)null);

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(distributedCache: fakeDistributedCache);

        var authenticationTicket = await distributedCacheTicketStore.RetrieveAsync(fakeSessionKey);

        Assert.Null(authenticationTicket);
    }

    [Fact]
    public async Task RenewAsync_Renews_Value_In_DistributedCache()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions();

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(fakeDistributedCacheTicketStoreOptions, fakeDistributedCache);
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket();

        await distributedCacheTicketStore.RenewAsync(fakeSessionKey, fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(fakeDistributedCache, fakeSessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RenewAsync_Renews_Value_In_DistributedCache_With_Specified_Options()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions
        {
            Expires = fakeExpires,
            SlidingExpiration = fakeSlidingExpiration
        };

        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(fakeDistributedCacheTicketStoreOptions, fakeDistributedCache);
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket();

        await distributedCacheTicketStore.RenewAsync(fakeSessionKey, fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(fakeDistributedCache, fakeSessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RemoveAsync_Removes_Value_From_Cache()
    {
        var fakeDistributedCache = A.Fake<IDistributedCache>();
        var distributedCacheTicketStore = CreateDistributedCacheTicketStore(distributedCache: fakeDistributedCache);

        await distributedCacheTicketStore.RemoveAsync(fakeSessionKey);

        A.CallTo(() => fakeDistributedCache.RemoveAsync(fakeSessionKey, A<CancellationToken>._))
            .MustHaveHappened();
    }

    private static IOptionsMonitor<DistributedCacheTicketStoreOptions> CreateFakeOptionsMonitor(DistributedCacheTicketStoreOptions? distributedCacheTicketStoreOptions = null)
        => TestFakes.OptionsMonitor(distributedCacheTicketStoreOptions);

    private static AuthenticationTicket CreateFakeAuthenticationTicket()
        => new(new ClaimsPrincipal(), new AuthenticationProperties(), fakeAuthenticationScheme);

    private static ITicketStore CreateDistributedCacheTicketStore(DistributedCacheTicketStoreOptions? distributedCacheTicketStoreOptions = null, IDistributedCache? distributedCache = null)
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton(CreateFakeOptionsMonitor(distributedCacheTicketStoreOptions))
            .AddSingleton(distributedCache ?? A.Dummy<IDistributedCache>())
            .AddCookiesDistributedCacheTicketStore();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme).SessionStore!;
    }

    private static void VerifyDistributedCacheSetAsync(
        IDistributedCache fakeDistributedCache,
        string expectedSessionKey,
        AuthenticationTicket expectedAuthenticationTicket,
        TimeSpan expectedSlidingExpiration)
    {
        A.CallTo(() => fakeDistributedCache.SetAsync(
                expectedSessionKey,
                A<byte[]>.That.Matches(
                    xBytes => xBytes.SequenceEqual(TicketSerializer.Default.Serialize(expectedAuthenticationTicket))),
                A<DistributedCacheEntryOptions>.That.Matches(
                    xOptions => xOptions.SlidingExpiration == expectedSlidingExpiration && xOptions.AbsoluteExpiration == expectedAuthenticationTicket.Properties.ExpiresUtc),
                A<CancellationToken>._))
            .MustHaveHappened();
    }
}
