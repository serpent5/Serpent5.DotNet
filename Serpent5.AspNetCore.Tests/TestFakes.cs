using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace Serpent5.AspNetCore.Tests;

internal static class TestFakes
{
    public static IMemoryCache MemoryCache()
        => new MemoryCache(Options.Create(new MemoryCacheOptions()));

    public static IWebHostEnvironment WebHostEnvironment()
        => Mock.Of<IWebHostEnvironment>();

    public static IOptionsMonitor<T> OptionsMonitor<T>(T? optionsValue = null)
        where T : class, new()
    {
        var mockOptionsMonitor = new Mock<IOptionsMonitor<T>>();

        mockOptionsMonitor.SetupGet(x => x.CurrentValue)
            .Returns(optionsValue ?? new T());

        return mockOptionsMonitor.Object;
    }

    public static HttpContext HttpContext()
        => new DefaultHttpContext();

    public static HttpContext HttpContextWithUserClaim(string claimType, string claimValue)
    {
        var httpContext = HttpContext();
        httpContext.User = ClaimsPrincipalWithClaim(claimType, claimValue);
        return httpContext;
    }

    public static ControllerContext ControllerContext()
        => new() { HttpContext = HttpContext() };

    private static ClaimsPrincipal ClaimsPrincipalWithClaim(string claimType, string claimValue)
        => new(new ClaimsIdentity(new[] { new Claim(claimType, claimValue) }, "fakeAuthenticationType"));
}
