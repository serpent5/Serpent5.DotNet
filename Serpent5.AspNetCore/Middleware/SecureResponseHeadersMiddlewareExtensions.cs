using JetBrains.Annotations;
using Serpent5.AspNetCore.Middleware;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IApplicationBuilder"/> extension methods for setting security response headers.
/// </summary>
[PublicAPI]
public static class SecureResponseHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that sets security response headers.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
    /// <returns>A reference to the provided <see cref="IApplicationBuilder" /> for a fluent API.</returns>
    public static IApplicationBuilder UseSecureResponseHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<SecureResponseHeadersMiddleware>();
}
