using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class WebApplicationBehaviorBuilder : IWebApplicationBehaviorBuilder
{
    [Flags]
    private enum WebApplicationBehavior : byte
    {
        // ReSharper disable InconsistentNaming
        None,
        Default = 1,
        WebAPI = 1 << 1
        // ReSharper restore InconsistentNaming
    }

    private readonly IDictionary<WebApplicationBehavior, Action<WebApplicationBuilder>> webApplicationBehaviorActions;
    private readonly List<Action<IServiceCollection>> configureServicesDelegates = new();

    private WebApplicationBehavior webApplicationBehaviors = WebApplicationBehavior.Default;

    private Action<SwaggerGenOptions>? configureSwaggerDelegate;

    public WebApplicationBehaviorBuilder(string appName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(appName);

        webApplicationBehaviorActions = new Dictionary<WebApplicationBehavior, Action<WebApplicationBuilder>>
        {
            [WebApplicationBehavior.Default] = webApplicationBuilder => ConfigureDefault(webApplicationBuilder, appName),
            [WebApplicationBehavior.WebAPI] = webApplicationBuilder => ConfigureWebAPI(webApplicationBuilder, configureSwaggerDelegate)
        };
    }

    public IWebApplicationBehaviorBuilder ConfigureWebAPI(Action<SwaggerGenOptions>? configureSwagger = null)
    {
        EnableBehavior(WebApplicationBehavior.WebAPI);
        configureSwaggerDelegate = configureSwagger;

        return this;
    }

    public IWebApplicationBehaviorBuilder ConfigureServices(Action<IServiceCollection> configureServices)
    {
        configureServicesDelegates.Add(configureServices);
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

        configureServicesDelegates.ForEach(configureServices => configureServices(webApplicationBuilder.Services));
    }

    private void EnableBehavior(WebApplicationBehavior webApplicationBehavior)
        => webApplicationBehaviors |= webApplicationBehavior;

    private static void ConfigureDefault(WebApplicationBuilder webApplicationBuilder, string appName)
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

    // ReSharper disable once InconsistentNaming
    private static void ConfigureWebAPI(WebApplicationBuilder webApplicationBuilder, Action<SwaggerGenOptions>? configureSwagger)
    {
        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            webApplicationBuilder.Services.AddEndpointsApiExplorer();
            webApplicationBuilder.Services.AddSwaggerGen();
        }

        // ReSharper disable RedundantNameQualifier
        ConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions, JsonOptionsSetup>(webApplicationBuilder);
        ConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions, JsonOptionsSetup>(webApplicationBuilder);
        // ReSharper restore RedundantNameQualifier

        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            ConfigureOptions<SwaggerGenOptions, SwaggerGenOptionsSetup>(webApplicationBuilder);
            ConfigureOptions<SwaggerUIOptions, SwaggerUIOptionsSetup>(webApplicationBuilder);

            if (configureSwagger is not null)
                webApplicationBuilder.Services.Configure(configureSwagger);
        }
    }

    private static void ConfigureOptions<TOptions, TOptionsSetup>(WebApplicationBuilder webApplicationBuilder)
        where TOptions : class
        where TOptionsSetup : class, IConfigureOptions<TOptions>
        => webApplicationBuilder.Services.AddTransient<IConfigureOptions<TOptions>, TOptionsSetup>();
}
