using JetBrains.Annotations;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// An interface for configuring <see cref="WebApplicationBuilder" /> Behaviors.
/// </summary>
/// <remarks>
/// A Behavior is a set of service-registrations and/or configuration that sets up a <see cref="WebApplicationBuilder" /> to behave as e.g. a Web API.
/// </remarks>
[PublicAPI]
public static class WebApplicationBehaviorBuilderExtensions
{
    /// <summary>
    /// Configures the <see cref="IWebApplicationBehaviorBuilder" /> to set up behavior for a Web API.
    /// </summary>
    /// <param name="webApplicationBehaviorBuilder"></param>
    /// <param name="configureSwagger">A <see cref="Action{T}"/> to configure the provided <see cref="SwaggerGenOptions" />.</param>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    public static IWebApplicationBehaviorBuilder ConfigureWebApi(this IWebApplicationBehaviorBuilder webApplicationBehaviorBuilder, Action<SwaggerGenOptions>? configureSwagger = null)
    {
        ArgumentNullException.ThrowIfNull(webApplicationBehaviorBuilder);

        return webApplicationBehaviorBuilder.AddBehavior(new WebApiWebApplicationBehavior(configureSwagger));
    }
}
