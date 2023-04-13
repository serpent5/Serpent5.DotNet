using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class DefaultWebApplicationBehavior : WebApplicationBehavior
{
    private readonly string appName;

    public DefaultWebApplicationBehavior(string appName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(appName);

        this.appName = appName;
    }

    public override void Configure(WebApplicationBuilder webApplicationBuilder)
    {
        webApplicationBuilder.Host.UseSerilog(static (ctx, sp, loggerConfiguration) =>
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
            .AddAuthenticatedUserIdTelemetryInitializer();

        webApplicationBuilder.Services.AddHealthChecks();

        if (webApplicationBuilder.Environment.IsDevelopment())
            webApplicationBuilder.Services.AddHttpForwarder();

        ConfigureOptions<KestrelServerOptions, KestrelServerOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<StaticFileOptions, StaticFileOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<CookiePolicyOptions, CookiePolicyOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<HealthCheckOptions, HealthCheckOptionsSetup>(webApplicationBuilder);

        if (!webApplicationBuilder.Environment.IsDevelopment())
        {
            ConfigureOptions<ExceptionHandlerOptions, ExceptionHandlerOptionsSetup>(webApplicationBuilder);
            ConfigureOptions<HstsOptions, HstsOptionsSetup>(webApplicationBuilder);
            ConfigureOptions<HttpsRedirectionOptions, HttpsRedirectionOptionsSetup>(webApplicationBuilder);
        }
    }
}
