using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Tests;

internal static class TestFakes
{
    // ReSharper disable InconsistentNaming

    /// <returns>
    /// A <see cref="string" /> that isn't null, empty, or whitespace.
    /// </returns>
    public const string String = "anyString";

    /// <returns>
    /// A <see cref="System.DateTimeOffset" /> that isn't set to its default value.
    /// </returns>
    public static DateTimeOffset DateTimeOffset()
        => new(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IHostEnvironment HostEnvironment()
    {
        var fakeEnvironment = A.Fake<IHostEnvironment>();
        fakeEnvironment.EnvironmentName = Environments.Production;
        return fakeEnvironment;
    }

    /// <returns>A <see cref="IOptionsMonitor{TOptions}" /> that returns <paramref name="optionsValue" /> or its <c>default</c>.</returns>
    public static IOptionsMonitor<T> OptionsMonitor<T>(T? optionsValue = null)
        where T : class, new()
    {
        var fakeOptionsMonitor = A.Fake<IOptionsMonitor<T>>();
        A.CallTo(() => fakeOptionsMonitor.CurrentValue).Returns(optionsValue ?? new T());
        return fakeOptionsMonitor;
    }

    /// <returns>
    /// An implementation of <see cref="IFileInfo" /> with the specified <paramref name="fileName" />.
    /// </returns>
    public static IFileInfo FileInfo(string? fileName = null)
    {
        var fakeFileInfo = A.Fake<IFileInfo>();
        A.CallTo<string?>(() => fakeFileInfo.Name).Returns(fileName);
        return fakeFileInfo;
    }

    /// <returns>
    /// An implementation of <see cref="IHttpContextAccessor" /> that returns the specified <paramref name="httpContext" />.
    /// </returns>
    public static IHttpContextAccessor HttpContextAccessor(HttpContext httpContext)
    {
        var fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
        A.CallTo(() => fakeHttpContextAccessor.HttpContext).Returns(httpContext);
        return fakeHttpContextAccessor;
    }

    /// <returns>
    /// An implementation of <see cref="Microsoft.AspNetCore.Http.HttpContext" />.
    /// </returns>
    public static HttpContext HttpContext()
        => new DefaultHttpContext();

    /// <returns>
    /// An implementation of <see cref="Microsoft.AspNetCore.Http.HttpContext" /> with its <see cref="ClaimsPrincipal" />
    /// configured to include the specified <paramref name="claimType" /> and <paramref name="claimValue" />.
    /// </returns>
    public static HttpContext HttpContextWithUserClaim(string claimType, string claimValue)
    {
        var httpContext = HttpContext();
        httpContext.User = ClaimsPrincipalWithClaim(claimType, claimValue);
        return httpContext;
    }

    /// <returns>
    /// An implementation of <see cref="Microsoft.AspNetCore.Mvc.ControllerContext" />.
    /// </returns>
    public static ControllerContext ControllerContext()
        => new() { HttpContext = HttpContext() };

    private static ClaimsPrincipal ClaimsPrincipalWithClaim(string claimType, string claimValue)
        => new(new ClaimsIdentity(new[] { new Claim(claimType, claimValue) }, String));
}
