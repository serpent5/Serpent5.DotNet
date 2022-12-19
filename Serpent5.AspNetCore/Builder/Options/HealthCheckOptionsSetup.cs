using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class HealthCheckOptionsSetup : IConfigureOptions<HealthCheckOptions>
{
    // Disable the default response e.g. "Healthy".
    public void Configure(HealthCheckOptions healthCheckOptions)
        => healthCheckOptions.ResponseWriter = (_, _) => Task.CompletedTask;
}
