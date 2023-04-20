// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Builder;

internal sealed class SecureResponseHeadersFeature
{
    public bool RequireTrustedTypes { get; set; } = true;
}
