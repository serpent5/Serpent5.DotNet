// ReSharper disable once CheckNamespace
namespace System.Security.Claims;

/// <summary>
/// Extensions for <see cref="ClaimsPrincipal" />.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the value of the <see cref="ClaimTypes.NameIdentifier" /> claim.
    /// </summary>
    /// <param name="claimsPrincipal">The <see cref="ClaimsPrincipal" /> to check for the claim.</param>
    /// <returns>The value of the <see cref="ClaimTypes.NameIdentifier" /> claim.</returns>
    /// <exception cref="InvalidOperationException">The <see cref="ClaimTypes.NameIdentifier" /> claim doesn't exist.</exception>
    public static string GetNameIdentifier(this ClaimsPrincipal claimsPrincipal)
    {
        ArgumentNullException.ThrowIfNull(claimsPrincipal);

        var nameIdentifier = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(nameIdentifier))
            throw new InvalidOperationException($"The \"{ClaimTypes.NameIdentifier}\" claim wasn't found.");

        return nameIdentifier;
    }
}
