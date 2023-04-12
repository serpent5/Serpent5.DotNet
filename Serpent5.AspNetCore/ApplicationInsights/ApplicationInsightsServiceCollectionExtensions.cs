using System.Security.Claims;
using Microsoft.ApplicationInsights.Extensibility;
using Serpent5.AspNetCore.ApplicationInsights;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extension methods for Application Insights.
/// </summary>
public static class ApplicationInsightsServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="ITelemetryInitializer" /> that sets the Cloud Role Name in Application Insights.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="cloudRoleName">The Cloud Role Name to use in Application Insights.</param>
    /// <returns>A reference to <paramref name="serviceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddCloudRoleNameTelemetryInitializer(this IServiceCollection serviceCollection, string cloudRoleName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(cloudRoleName);

        return serviceCollection.AddSingleton<ITelemetryInitializer>(_ => new CloudRoleNameTelemetryInitializer(cloudRoleName));
    }

    /// <summary>
    /// Adds an <see cref="ITelemetryInitializer" /> that sets the Authenticated User ID in Application Insights to the
    /// <see cref="Microsoft.AspNetCore.Http.HttpContext.User" />'s <see cref="ClaimTypes.NameIdentifier" /> claim value.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <returns>A reference to <paramref name="serviceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddAuthenticatedUserIdTelemetryInitializer(this IServiceCollection serviceCollection)
        => serviceCollection.AddSingleton<ITelemetryInitializer>(
            sp => ActivatorUtilities.CreateInstance<AuthenticatedUserIdTelemetryInitializer>(sp, ClaimTypes.NameIdentifier));
}
