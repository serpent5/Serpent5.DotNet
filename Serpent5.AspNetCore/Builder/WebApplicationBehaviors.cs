using System.Globalization;
using System.Security.Claims;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serpent5.AspNetCore.Builder.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// A set of behaviors for configuring a <see cref="WebApplication" /> to behave as e.g. a Web API.
/// </summary>
[PublicAPI]
public class WebApplicationBehaviors
{
    [Flags]
    private enum WebApplicationBehavior : byte
    {
        None,
        Default = 0x01
    }

    private readonly IDictionary<WebApplicationBehavior, Action<WebApplicationBuilder>> webApplicationBehaviorActions;

    private WebApplicationBehavior webApplicationBehaviors = WebApplicationBehavior.Default;

    private WebApplicationBehaviors(string appName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(appName);

        webApplicationBehaviorActions = new Dictionary<WebApplicationBehavior, Action<WebApplicationBuilder>>
        {
            [WebApplicationBehavior.Default] = x => ConfigureDefault(x, appName)
        };
    }

    /// <summary>
    /// Initializes a new instance of <see cref="WebApplicationBehaviors" />.
    /// </summary>
    /// <param name="appName">The name of the app, as shown in e.g. Application Insights.</param>
    /// <returns>A reference to the provided <see cref="WebApplicationBehaviors" /> for a fluent API.</returns>
    public static WebApplicationBehaviors Create(string appName)
        => new(appName);

    /// <summary>
    /// Configures a <see cref="WebApplicationBuilder" /> with the behaviors attached to this <see cref="WebApplicationBehaviors" />.
    /// </summary>
    /// <param name="webApplicationBuilder">The <see cref="WebApplicationBuilder" />to configure.</param>
    public void Configure(WebApplicationBuilder webApplicationBuilder)
    {
        foreach (var webApplicationBehavior in Enum.GetValues<WebApplicationBehavior>())
        {
            if (webApplicationBehavior is WebApplicationBehavior.None)
                continue;

            if ((webApplicationBehaviors & webApplicationBehavior) == webApplicationBehavior)
                webApplicationBehaviorActions[webApplicationBehavior](webApplicationBuilder);
        }
    }

    private static void ConfigureDefault(WebApplicationBuilder webApplicationBuilder, string appName)
    {
        webApplicationBuilder.Host.UseSerilog((ctx, sp, loggerConfiguration) =>
        {
            loggerConfiguration.WriteTo.Console(LogEventLevel.Information, formatProvider: CultureInfo.InvariantCulture, theme: AnsiConsoleTheme.Code);

            if (ctx.HostingEnvironment.IsDevelopment())
                loggerConfiguration.WriteTo.Seq("http://localhost:5341");

            // If it looks like the app's running in Azure App Service, write to the standard App Service Logs folder.
            if (!string.IsNullOrWhiteSpace(ctx.Configuration["WEBSITE_SITE_NAME"]) && !string.IsNullOrWhiteSpace(ctx.Configuration["HOME"]))
            {
                loggerConfiguration.WriteTo.File(
                    Path.Combine(ctx.Configuration["HOME"]!, "LogFiles", "Application", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    formatProvider: CultureInfo.InvariantCulture);
            }

            loggerConfiguration
                .ReadFrom.Services(sp)
                .ReadFrom.Configuration(ctx.Configuration);
        });

        webApplicationBuilder.Services.AddApplicationInsightsTelemetry()
            .AddCloudRoleNameTelemetryInitializer(appName)
            .AddAuthenticatedUserIdTelemetryInitializer(ClaimTypes.NameIdentifier);

        webApplicationBuilder.Services.AddHealthChecks();

        webApplicationBuilder.Services
            .AddTransient<IConfigureOptions<KestrelServerOptions>, KestrelServerOptionsSetup>()
            .AddTransient<IConfigureOptions<RouteOptions>, RouteOptionsSetup>()
            .AddTransient<IConfigureOptions<HealthCheckOptions>, HealthCheckOptionsSetup>()
            .AddTransient<IConfigureOptions<CookiePolicyOptions>, CookiePolicyOptionsSetup>();

        if (!webApplicationBuilder.Environment.IsDevelopment())
        {
            webApplicationBuilder.Services
                .AddTransient<IConfigureOptions<ExceptionHandlerOptions>, ExceptionHandlerOptionsSetup>()
                .AddTransient<IConfigureOptions<HttpsRedirectionOptions>, HttpsRedirectionOptionsSetup>()
                .AddTransient<IConfigureOptions<HstsOptions>, HstsOptionsSetup>();
        }
    }
}
