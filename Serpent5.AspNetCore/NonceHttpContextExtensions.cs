using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Http;

internal static class NonceHttpContextExtensions
{
    private const string nonceItemName = "Serpent5.Nonce";

    public static string GetNonce(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (httpContext.TryGetNonce(out var nonceValue))
            return nonceValue;

        nonceValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        httpContext.Items[nonceItemName] = nonceValue;

        return nonceValue;
    }

    public static bool TryGetNonce(this HttpContext httpContext, [NotNullWhen(true)] out string? nonceValue)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (!httpContext.Items.TryGetValue(nonceItemName, out var nonceValueAsObject))
        {
            nonceValue = null;
            return false;
        }

        nonceValue = (string)nonceValueAsObject!;
        return true;
    }
}
