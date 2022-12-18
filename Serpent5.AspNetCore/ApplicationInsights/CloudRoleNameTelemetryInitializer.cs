using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serpent5.AspNetCore.ApplicationInsights;

internal sealed class CloudRoleNameTelemetryInitializer : ITelemetryInitializer
{
    private readonly string cloudRoleName;

    public CloudRoleNameTelemetryInitializer(string cloudRoleName)
        => this.cloudRoleName = cloudRoleName;

    public void Initialize(ITelemetry telemetryItem)
        => telemetryItem.Context.Cloud.RoleName = cloudRoleName;
}
