using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Moq;
using Serpent5.AspNetCore.Authentication;

namespace Serpent5.AspNetCore.Tests.Authentication;

public class DistributedCacheTicketStoreTests
{
    private const string anyAuthenticationScheme = "anyString";
    private const string anySessionKey = "anyString";

    private static readonly DateTime currentDateTime = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static readonly TimeSpan anyExpires = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan anySlidingExpiration = TimeSpan.FromSeconds(1);

    [Fact]
    public async Task StoreAsync_CreatesValidSessionKey()
    {
        const int expectedSessionKeyByteCount = 32;

        var distributedCacheTicketStore = new DistributedCacheTicketStore(Mock.Of<IDistributedCache>(), CreateFakeOptionsMonitor());
        var fakeAuthenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), anyAuthenticationScheme);

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        Assert.NotEmpty(sessionKey);
        Assert.Equal(expectedSessionKeyByteCount, Convert.FromBase64String(sessionKey).Length);
    }

    [Fact]
    public async Task StoreAsync_StoresInDistributedCache()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions();
        var fakeOptionsMonitor = CreateFakeOptionsMonitor(fakeDistributedCacheTicketStoreOptions);

        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, fakeOptionsMonitor);
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket(fakeDistributedCacheTicketStoreOptions.Expires);

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(mockDistributedCache, sessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task StoreAsync_StoresInDistributedCacheWithSpecifiedOptions()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions
        {
            Expires = anyExpires,
            SlidingExpiration = anySlidingExpiration
        };

        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, CreateFakeOptionsMonitor(fakeDistributedCacheTicketStoreOptions));
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket(fakeDistributedCacheTicketStoreOptions.Expires);

        var sessionKey = await distributedCacheTicketStore.StoreAsync(fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(mockDistributedCache, sessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RetrieveAsync_RetrievesAuthenticationTicketFromCache()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, CreateFakeOptionsMonitor());

        mockDistributedCache
            .Setup(x => x.GetAsync(anySessionKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: null);

        var authenticationTicket = await distributedCacheTicketStore.RetrieveAsync(anySessionKey);

        Assert.Null(authenticationTicket);
    }

    [Fact]
    public async Task RenewAsync_RenewsInDistributedCache()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions();

        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, CreateFakeOptionsMonitor(fakeDistributedCacheTicketStoreOptions));
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket(fakeDistributedCacheTicketStoreOptions.Expires);

        await distributedCacheTicketStore.RenewAsync(anySessionKey, fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(mockDistributedCache, anySessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RenewAsync_RenewsInDistributedCacheWithSpecifiedOptions()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var fakeDistributedCacheTicketStoreOptions = new DistributedCacheTicketStoreOptions
        {
            Expires = anyExpires,
            SlidingExpiration = anySlidingExpiration
        };

        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, CreateFakeOptionsMonitor(fakeDistributedCacheTicketStoreOptions));
        var fakeAuthenticationTicket = CreateFakeAuthenticationTicket(fakeDistributedCacheTicketStoreOptions.Expires);

        await distributedCacheTicketStore.RenewAsync(anySessionKey, fakeAuthenticationTicket);

        VerifyDistributedCacheSetAsync(mockDistributedCache, anySessionKey, fakeAuthenticationTicket, fakeDistributedCacheTicketStoreOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RemoveAsync_RemovesAuthenticationTicketFromCache()
    {
        var mockDistributedCache = new Mock<IDistributedCache>();
        var distributedCacheTicketStore = new DistributedCacheTicketStore(mockDistributedCache.Object, CreateFakeOptionsMonitor());

        await distributedCacheTicketStore.RemoveAsync(anySessionKey);

        mockDistributedCache.Verify(x => x.RemoveAsync(anySessionKey, It.IsAny<CancellationToken>()));
    }

    private static IOptionsMonitor<DistributedCacheTicketStoreOptions> CreateFakeOptionsMonitor(DistributedCacheTicketStoreOptions? distributedCacheTicketStoreOptions = null)
        => TestFakes.OptionsMonitor(distributedCacheTicketStoreOptions);

    private static AuthenticationTicket CreateFakeAuthenticationTicket(TimeSpan expiresUtc)
        => new(
            new ClaimsPrincipal(),
            new AuthenticationProperties { ExpiresUtc = currentDateTime.Add(expiresUtc) },
            anyAuthenticationScheme);

    private static void VerifyDistributedCacheSetAsync(
        Mock<IDistributedCache> mockDistributedCache, string expectedSessionKey, AuthenticationTicket expectedAuthenticationTicket, TimeSpan expectedSlidingExpiration)
        => mockDistributedCache.Verify(
            x => x.SetAsync(
                expectedSessionKey,
                TicketSerializer.Default.Serialize(expectedAuthenticationTicket),
                // ReSharper disable once VariableHidesOuterVariable
                It.Is<DistributedCacheEntryOptions>(x => x.SlidingExpiration == expectedSlidingExpiration && x.AbsoluteExpiration == expectedAuthenticationTicket.Properties.ExpiresUtc),
                It.IsAny<CancellationToken>()));
}
