using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
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
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Serpent5.AspNetCore.Builder;

internal class WebApplicationBehaviorBuilder : IWebApplicationBehaviorBuilder
{
    [Flags]
    private enum WebApplicationBehavior : byte
    {
        None,
        Default = 0x01,
        // ReSharper disable once InconsistentNaming
        WebAPI = 0x02
    }

    private readonly IDictionary<WebApplicationBehavior, Action<WebApplicationBuilder>> webApplicationBehaviorActions;
    private WebApplicationBehavior webApplicationBehaviors = WebApplicationBehavior.Default;

    public WebApplicationBehaviorBuilder(string appName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(appName);

        webApplicationBehaviorActions = new Dictionary<WebApplicationBehavior, Action<WebApplicationBuilder>>
        {
            [WebApplicationBehavior.Default] = webApplicationBuilder => ConfigureDefault(webApplicationBuilder, appName),
            [WebApplicationBehavior.WebAPI] = ConfigureWebAPI
        };
    }

    public IWebApplicationBehaviorBuilder ConfigureWebAPI()
    {
        AddBehavior(WebApplicationBehavior.WebAPI);
        return this;
    }

    internal void Configure(WebApplicationBuilder webApplicationBuilder)
    {
        foreach (var webApplicationBehavior in Enum.GetValues<WebApplicationBehavior>())
        {
            if (webApplicationBehavior is WebApplicationBehavior.None)
                continue;

            if (webApplicationBehaviors.HasFlag(webApplicationBehavior))
                webApplicationBehaviorActions[webApplicationBehavior](webApplicationBuilder);
        }
    }

    private void AddBehavior(WebApplicationBehavior webApplicationBehavior)
        => webApplicationBehaviors |= webApplicationBehavior;

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

    // ReSharper disable once InconsistentNaming
    private static void ConfigureWebAPI(WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            webApplicationBuilder.Services.AddSwaggerGen();

            var xmlCommentsFilename = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            if (File.Exists(xmlCommentsFilename))
                webApplicationBuilder.Services.Configure<SwaggerGenOptions>(o => o.IncludeXmlComments(xmlCommentsFilename));
        }

        webApplicationBuilder.Services.AddTransient<IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>, JsonOptionsSetup>();
        webApplicationBuilder.Services.AddTransient<IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>, JsonOptionsSetup>();
    }
}