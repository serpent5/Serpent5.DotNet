using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal class WebApiWebApplicationBehavior : WebApplicationBehavior
{
    private readonly Action<SwaggerGenOptions>? configureSwaggerDelegate;

    public WebApiWebApplicationBehavior(Action<SwaggerGenOptions>? configureSwagger = null)
        => configureSwaggerDelegate = configureSwagger;

    public override void Configure(WebApplicationBuilder webApplicationBuilder)
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

            if (configureSwaggerDelegate is not null)
                webApplicationBuilder.Services.Configure(configureSwaggerDelegate);
        }
    }
}
