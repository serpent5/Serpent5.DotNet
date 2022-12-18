using Serpent5.AspNetCore.ApplicationInsights;

namespace Serpent5.AspNetCore.Tests.ApplicationInsights;

public class CloudRoleNameTelemetryInitializerTests
{
    [Fact]
    public void SetsCloudRoleNameToConstructorParameter()
    {
        const string expectedCloudRoleName = "anyCloudRole";

        var cloudRoleNameTelemetryInitializer = new CloudRoleNameTelemetryInitializer(expectedCloudRoleName);
        var fakeTelemetryItem = ApplicationInsightsTestFakes.TelemetryItem();

        cloudRoleNameTelemetryInitializer.Initialize(fakeTelemetryItem);

        Assert.Equal(expectedCloudRoleName, fakeTelemetryItem.Context.Cloud.RoleName);
    }
}
