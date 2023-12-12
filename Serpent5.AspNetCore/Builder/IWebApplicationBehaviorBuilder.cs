using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// An interface for configuring <see cref="WebApplicationBuilder" /> Behaviors.
/// </summary>
/// <remarks>
/// A Behavior is a set of service-registrations and/or configuration that sets up a <see cref="WebApplicationBuilder" /> to behave as e.g. a Web API.
/// </remarks>
[PublicAPI]
public interface IWebApplicationBehaviorBuilder
{
    /// <summary>
    /// Configures services for Dependency Injection.
    /// </summary>
    /// <param name="configureServices">A <see cref="Action{T}"/> to configure the provided <see cref="IServiceCollection" />.</param>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    IWebApplicationBehaviorBuilder ConfigureServices(Action<IServiceCollection> configureServices);

    internal IWebApplicationBehaviorBuilder AddBehavior(WebApplicationBehavior webApplicationBehavior);
}
