using JetBrains.Annotations;
using Serpent5.AspNetCore.Middleware;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IApplicationBuilder"/> extension methods for adding cache response headers.
/// </summary>
[PublicAPI]
public static class CacheResponseHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that sets response headers for <c>Cache-Control</c> and clears redundant cache headers.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
    /// <returns>A reference to the provided <see cref="IApplicationBuilder" /> for a fluent API.</returns>
    public static IApplicationBuilder UseCacheResponseHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<CacheResponseHeadersMiddleware>();
}
