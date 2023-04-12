// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IApplicationBuilder" /> extension methods for controlling cache response headers.
/// </summary>
public static class CacheResponseHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that sets response headers for <c>Cache-Control</c> and clears redundant cache headers.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
    /// <returns>A reference to <paramref name="applicationBuilder" /> for a fluent API.</returns>
    public static IApplicationBuilder UseCacheResponseHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<CacheResponseHeadersMiddleware>();
}
