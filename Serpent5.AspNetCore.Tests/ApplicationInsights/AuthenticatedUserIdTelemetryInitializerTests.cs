using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Moq;
using Serpent5.AspNetCore.ApplicationInsights;

namespace Serpent5.AspNetCore.Tests.ApplicationInsights;

public class AuthenticatedUserIdTelemetryInitializerTests
{
    [Theory]
    [InlineData("anyClaim", "anyValue")]
    [InlineData("anyOtherClaim", "anyOtherValue")]
    public void SetsAuthenticatedUserIdToClaimValue(string claimType, string claimValue)
    {
        var fakeHttpContext = TestFakes.HttpContextWithUserClaim(claimType, claimValue);

        fakeHttpContext.Features.Set(new RequestTelemetry());

        Assert.True(fakeHttpContext.User.Identity!.IsAuthenticated);

        var fakeHttpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == fakeHttpContext);
        var authenticatedUserIdTelemetryInitializer = new AuthenticatedUserIdTelemetryInitializer(claimType, fakeHttpContextAccessor);
        var fakeTelemetryItem = ApplicationInsightsTestFakes.TelemetryItem();

        authenticatedUserIdTelemetryInitializer.Initialize(fakeTelemetryItem);

        Assert.Equal(claimValue, fakeTelemetryItem.Context.User.AuthenticatedUserId);
    }
}

