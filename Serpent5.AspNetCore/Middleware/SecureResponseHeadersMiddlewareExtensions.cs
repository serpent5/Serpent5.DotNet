// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// <see cref="IApplicationBuilder" /> extension methods for setting secure response headers.
/// </summary>
public static class SecureResponseHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds middleware that sets secure response headers.
    /// </summary>
    /// <param name="applicationBuilder">The <see cref="IApplicationBuilder" /> to add the middleware to.</param>
    /// <returns>A reference to <paramref name="applicationBuilder" /> for a fluent API.</returns>
    public static IApplicationBuilder UseSecureResponseHeaders(this IApplicationBuilder applicationBuilder)
        => applicationBuilder.UseMiddleware<SecureResponseHeadersMiddleware>();
}
