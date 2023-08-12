// ReSharper disable once CheckNamespace

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides options for <see cref="SecureResponseHeadersMiddleware" />.
/// </summary>
public sealed class SecureResponseHeadersOptions
{
    /// <summary>
    /// Gets or sets whether to include 'unsafe-eval' in the CSP's script-src directives.
    /// </summary>
    public bool UnsafeEval { get; set; }

    internal ICollection<string> TrustedTypesPolicies { get; } = new SortedSet<string>();

    /// <summary>
    /// Adds a set of Trusted Types policies to allow with Content Security Policy (CSP).
    /// </summary>
    /// <remarks>See https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Security-Policy/trusted-types.</remarks>
    public void AddTrustedTypesPolicies(string trustedTypesPolicy, params string[] trustedTypesPolicies)
    {
        ArgumentNullException.ThrowIfNull(trustedTypesPolicies);

        TrustedTypesPolicies.Add(trustedTypesPolicy);

        foreach (var x in trustedTypesPolicies)
            TrustedTypesPolicies.Add(x);
    }
}
