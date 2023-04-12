using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class HstsOptionsSetup : IConfigureOptions<HstsOptions>
{
    public void Configure(HstsOptions hstsOptions)
    {
        // https://hstspreload.org/#deployment-recommendations
        hstsOptions.MaxAge = TimeSpan.FromSeconds(63072000); // 2 Years.
        hstsOptions.IncludeSubDomains = true;
        hstsOptions.Preload = true;
    }
}
