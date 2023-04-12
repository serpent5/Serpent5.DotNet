using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// An interface for configuring <see cref="WebApplicationBuilder" /> Behaviors.
/// </summary>
/// <remarks>
/// A Behavior is a set of service-registrations and/or configuration that sets up a <see cref="WebApplicationBuilder" /> to behave as e.g. a Web API.
/// </remarks>
public interface IWebApplicationBehaviorBuilder
{
    /// <summary>
    /// Configures the <see cref="IWebApplicationBehaviorBuilder" /> to set up behavior for a Web API.
    /// </summary>
    /// <param name="configureSwagger">An <see cref="Action{T}"/> to configure the provided <see cref="SwaggerGenOptions" />.</param>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    // ReSharper disable once InconsistentNaming
    IWebApplicationBehaviorBuilder ConfigureWebAPI(Action<SwaggerGenOptions>? configureSwagger = null);

    /// <summary>
    /// Configures services for Dependency Injection.
    /// </summary>
    /// <param name="configureServices">An <see cref="Action{T}"/> to configure the provided <see cref="IServiceCollection" />.</param>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    IWebApplicationBehaviorBuilder ConfigureServices(Action<IServiceCollection> configureServices);
}
