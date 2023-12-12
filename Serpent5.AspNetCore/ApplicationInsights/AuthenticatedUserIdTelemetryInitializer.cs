using System.Security.Claims;
using Microsoft.ApplicationInsights.AspNetCore.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace Serpent5.AspNetCore.ApplicationInsights;

internal sealed class AuthenticatedUserIdTelemetryInitializer(string claimType, IHttpContextAccessor httpContextAccessor)
    : TelemetryInitializerBase(httpContextAccessor)
{
    protected override void OnInitializeTelemetry(HttpContext httpContext, RequestTelemetry requestTelemetry, ITelemetry telemetryItem)
        => telemetryItem.Context.User.AuthenticatedUserId = httpContext.User.FindFirstValue(claimType);
}
