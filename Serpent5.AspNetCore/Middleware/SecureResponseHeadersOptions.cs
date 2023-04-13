// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides options for <see cref="SecureResponseHeadersMiddleware" />.
/// </summary>
public sealed class SecureResponseHeadersOptions
{
    /// <summary>
    /// A list of Trusted Types policies to allow (used with Content Security Policy (CSP)).
    /// </summary>
    /// <remarks>See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/trusted-types.</remarks>
    public IList<string> TrustedTypesPolicies { get; } = new List<string>();
}
