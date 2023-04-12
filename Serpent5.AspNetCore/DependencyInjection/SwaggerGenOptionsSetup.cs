using System.Reflection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class SwaggerGenOptionsSetup : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions swaggerGenOptions)
    {
        swaggerGenOptions.SupportNonNullableReferenceTypes();

        if (Assembly.GetEntryAssembly() is not { } entryAssembly)
            return;

        var xmlCommentsFilename = Path.Combine(AppContext.BaseDirectory, $"{entryAssembly.GetName().Name}.xml");

        if (File.Exists(xmlCommentsFilename))
            swaggerGenOptions.IncludeXmlComments(xmlCommentsFilename);
    }
}
