using System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.ApplicationInsights;

public class ApplicationInsightsServiceCollectionExtensionsTests
{
    [Fact]
    public void Sets_Cloud_RoleName()
    {
        const string expectedCloudRoleName = TestFakes.String;

        var cloudRoleNameTelemetryInitializer = new ServiceCollection()
            .AddCloudRoleNameTelemetryInitializer(expectedCloudRoleName)
            .BuildServiceProvider()
            .GetRequiredService<ITelemetryInitializer>();

        var fakeTelemetryItem = CreateFakeTelemetryItem();

        cloudRoleNameTelemetryInitializer.Initialize(fakeTelemetryItem);

        Assert.Equal(expectedCloudRoleName, fakeTelemetryItem.Context.Cloud.RoleName);
    }

    [Fact]
    public void Sets_User_AuthenticatedUserId()
    {
        const string fakeNameIdentifier = TestFakes.String;
        var fakeHttpContext = TestFakes.HttpContextWithUserClaim(ClaimTypes.NameIdentifier, fakeNameIdentifier);

        fakeHttpContext.Features.Set(new RequestTelemetry());

        var authenticatedUserIdTelemetryInitializer = new ServiceCollection()
            .AddSingleton(TestFakes.HttpContextAccessor(fakeHttpContext))
            .AddAuthenticatedUserIdTelemetryInitializer()
            .BuildServiceProvider()
            .GetRequiredService<ITelemetryInitializer>();

        var fakeTelemetryItem = CreateFakeTelemetryItem();

        authenticatedUserIdTelemetryInitializer.Initialize(fakeTelemetryItem);

        Assert.Equal(fakeNameIdentifier, fakeTelemetryItem.Context.User.AuthenticatedUserId);
    }

    private static ITelemetry CreateFakeTelemetryItem()
        => new TraceTelemetry(); // An out-of-the-box implementation avoids unnecessary and complex mocking.
}
