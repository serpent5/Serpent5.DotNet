using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Extension methods for adding configuration sources that mimic the defaults set up by <c>Host</c>, <c>WebHost</c>, and <c>WebApplication</c>.
/// </summary>
[PublicAPI]
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    /// Adds a set of <see cref="IConfigurationProvider" />s to mimic the default app configuration set up by <c>Host</c>, <c>WebHost</c>, and <c>WebApplication</c>.
    /// </summary>
    /// <param name="configurationBuilder">The <see cref="IConfigurationBuilder" /> to add to.</param>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
    public static IConfigurationBuilder AddHostingDefaults(this IConfigurationBuilder configurationBuilder, string[]? args = null)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);

        var dotnetEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environments.Production;

        configurationBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{dotnetEnvironment}.json", optional: true, reloadOnChange: true);

        if (dotnetEnvironment == Environments.Development)
        {
            var entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly is not null)
                configurationBuilder.AddUserSecrets(entryAssembly, optional: true);
        }

        configurationBuilder.AddEnvironmentVariables();

        if (args is not null)
            configurationBuilder.AddCommandLine(args);

        return configurationBuilder;
    }
}
