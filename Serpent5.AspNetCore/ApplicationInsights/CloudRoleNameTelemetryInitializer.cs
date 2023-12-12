using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Serpent5.AspNetCore.ApplicationInsights;

internal sealed class CloudRoleNameTelemetryInitializer(string cloudRoleName)
    : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetryItem)
        => telemetryItem.Context.Cloud.RoleName = cloudRoleName;
}
