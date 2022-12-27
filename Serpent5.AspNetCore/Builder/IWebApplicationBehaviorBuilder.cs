using JetBrains.Annotations;

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
    /// Configures the <see cref="IWebApplicationBehaviorBuilder" /> to set up behavior for a Web API.
    /// </summary>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    // ReSharper disable once InconsistentNaming
    IWebApplicationBehaviorBuilder ConfigureWebAPI();

    /// <summary>
    /// Configures the <see cref="IWebApplicationBehaviorBuilder" /> to set up behavior for a Server-Rendered UI (e.g. MVC, RP).
    /// </summary>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    // ReSharper disable once InconsistentNaming
    IWebApplicationBehaviorBuilder ConfigureServerUI();

    /// <summary>
    /// Configures the <see cref="IWebApplicationBehaviorBuilder" /> to set up behavior for a Client-Rendered UI (e.g. Angular).
    /// </summary>
    /// <returns>A reference to <c>this</c> for a fluent API.</returns>
    // ReSharper disable once InconsistentNaming
    IWebApplicationBehaviorBuilder ConfigureClientUI();
}
