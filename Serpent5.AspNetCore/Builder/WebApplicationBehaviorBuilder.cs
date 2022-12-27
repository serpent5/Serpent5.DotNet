using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        // ReSharper disable InconsistentNaming
        None,
        Default = 1,
        WebAPI = 1 << 1,
        ServerUI = 1 << 2,
        ClientUI = 1 << 3
        // ReSharper restore InconsistentNaming
    }

    private readonly IDictionary<WebApplicationBehavior, Action<WebApplicationBuilder>> webApplicationBehaviorActions;
    private WebApplicationBehavior webApplicationBehaviors = WebApplicationBehavior.Default;

    public WebApplicationBehaviorBuilder(string appName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(appName);

        webApplicationBehaviorActions = new Dictionary<WebApplicationBehavior, Action<WebApplicationBuilder>>
        {
            [WebApplicationBehavior.Default] = webApplicationBuilder => ConfigureDefault(webApplicationBuilder, appName),
            [WebApplicationBehavior.WebAPI] = ConfigureWebAPI,
            [WebApplicationBehavior.ServerUI] = ConfigureServerUI,
            [WebApplicationBehavior.ClientUI] = ConfigureClientUI
        };
    }

    public IWebApplicationBehaviorBuilder ConfigureWebAPI()
        => AddBehavior(WebApplicationBehavior.WebAPI);

    public IWebApplicationBehaviorBuilder ConfigureServerUI()
        => AddBehavior(WebApplicationBehavior.ServerUI);

    public IWebApplicationBehaviorBuilder ConfigureClientUI()
        => AddBehavior(WebApplicationBehavior.ClientUI);

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

    private IWebApplicationBehaviorBuilder AddBehavior(WebApplicationBehavior webApplicationBehavior)
    {
        webApplicationBehaviors |= webApplicationBehavior;
        return this;
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

        ConfigureOptions<KestrelServerOptions, KestrelServerOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<RouteOptions, RouteOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<HealthCheckOptions, HealthCheckOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<CookiePolicyOptions, CookiePolicyOptionsSetup>(webApplicationBuilder);

        if (!webApplicationBuilder.Environment.IsDevelopment())
        {
            ConfigureOptions<ExceptionHandlerOptions, ExceptionHandlerOptionsSetup>(webApplicationBuilder);
            ConfigureOptions<HttpsRedirectionOptions, HttpsRedirectionOptionsSetup>(webApplicationBuilder);
            ConfigureOptions<HstsOptions, HstsOptionsSetup>(webApplicationBuilder);
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

        ConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions, JsonOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions, JsonOptionsSetup>(webApplicationBuilder);
    }

    // ReSharper disable once InconsistentNaming
    private static void ConfigureServerUI(WebApplicationBuilder webApplicationBuilder)
        => ConfigureCommonUI(webApplicationBuilder);

    // ReSharper disable once InconsistentNaming
    private static void ConfigureClientUI(WebApplicationBuilder webApplicationBuilder)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
            webApplicationBuilder.Services.AddHttpForwarder();

        ConfigureCommonUI(webApplicationBuilder);
    }

    // ReSharper disable once InconsistentNaming
    private static void ConfigureCommonUI(WebApplicationBuilder webApplicationBuilder)
    {
        ConfigureOptions<StaticFileOptions, StaticFileOptionsSetup>(webApplicationBuilder);

        if (!webApplicationBuilder.Environment.IsDevelopment())
            ConfigureOptions<StaticFileOptions, StaticFileOptionsProductionSetup>(webApplicationBuilder);
    }

    private static void ConfigureOptions<TOptions, TOptionsSetup>(WebApplicationBuilder webApplicationBuilder)
        where TOptions : class
        where TOptionsSetup : class, IConfigureOptions<TOptions>
        => webApplicationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<TOptions>, TOptionsSetup>());
}
