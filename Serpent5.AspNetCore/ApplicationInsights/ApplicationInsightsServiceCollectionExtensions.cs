using JetBrains.Annotations;
using Microsoft.ApplicationInsights.Extensibility;
using Serpent5.AspNetCore.ApplicationInsights;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extension methods for Application Insights.
/// </summary>
[PublicAPI]
public static class ApplicationInsightsServiceCollectionExtensions
{
    /// <summary>
    /// Adds an <see cref="ITelemetryInitializer" /> that sets the Cloud Role Name in Application Insights.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="cloudRoleName">The Cloud Role Name to use in Application Insights.</param>
    /// <returns>A reference to the provided <see cref="IServiceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddCloudRoleNameTelemetryInitializer(this IServiceCollection serviceCollection, string cloudRoleName)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(cloudRoleName);

        return serviceCollection.AddSingleton<ITelemetryInitializer>(_ => new CloudRoleNameTelemetryInitializer(cloudRoleName));
    }

    /// <summary>
    /// Adds an <see cref="ITelemetryInitializer" /> that sets the Authenticated User ID in Application Insights to the
    /// <see cref="Microsoft.AspNetCore.Http.HttpContext.User" />'s <paramref name="claimType" /> claim value.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the service to.</param>
    /// <param name="claimType">The Claim Type to use for the Authenticated User ID.</param>
    /// <returns>A reference to the provided <see cref="IServiceCollection" /> for a fluent API.</returns>
    public static IServiceCollection AddAuthenticatedUserIdTelemetryInitializer(this IServiceCollection serviceCollection, string claimType)
    {
        ArgumentExceptionExtensions.ThrowIfNullOrWhiteSpace(claimType);

        return serviceCollection.AddSingleton<ITelemetryInitializer>(
            sp => ActivatorUtilities.CreateInstance<AuthenticatedUserIdTelemetryInitializer>(sp, claimType));
    }
}
