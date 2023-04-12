using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class HealthCheckOptionsSetup : IConfigureOptions<HealthCheckOptions>
{
    // Disable the default response, e.g. "Healthy", which offers no value.
    public void Configure(HealthCheckOptions healthCheckOptions)
        => healthCheckOptions.ResponseWriter = (_, _) => Task.CompletedTask;
}
