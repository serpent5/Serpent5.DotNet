// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="WebApplicationBuilder" /> extension methods for configuring Behaviors.
/// </summary>
public static class WebApplicationBuilderBehaviorExtensions
{
    /// <summary>
    /// Configures a <see cref="WebApplicationBuilder" /> to use Behaviors.
    /// </summary>
    /// <param name="webApplicationBuilder">The <see cref="WebApplicationBuilder" /> to configure.</param>
    /// <param name="appName">The name of the app, as shown in e.g. Application Insights.</param>
    /// <param name="configureBehaviorBuilder">The action used to configure one or more Behaviors.</param>
    /// <returns>A reference to <paramref name="webApplicationBuilder" /> for a fluent API.</returns>
    /// <remarks>
    /// A Behavior is a set of service-registrations and/or configuration that sets up a <see cref="WebApplicationBuilder" /> to behave as e.g. a Web API.
    /// </remarks>
    public static WebApplicationBuilder ConfigureBehavior(
        this WebApplicationBuilder webApplicationBuilder,
        string appName,
        Action<IWebApplicationBehaviorBuilder>? configureBehaviorBuilder = null)
    {
        ArgumentNullException.ThrowIfNull(webApplicationBuilder);

        var webApplicationBehaviorBuilder = new WebApplicationBehaviorBuilder(appName);

        configureBehaviorBuilder?.Invoke(webApplicationBehaviorBuilder);

        webApplicationBehaviorBuilder.Configure(webApplicationBuilder);

        return webApplicationBuilder;
    }
}
