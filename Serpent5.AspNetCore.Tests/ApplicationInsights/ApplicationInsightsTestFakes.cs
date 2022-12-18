using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Serpent5.AspNetCore.Tests.ApplicationInsights;

internal static class ApplicationInsightsTestFakes
{
    // Create ANY implementation of ITelemetry to avoid unnecessary and complex mocking.
    public static ITelemetry TelemetryItem()
        => new TraceTelemetry();
}
