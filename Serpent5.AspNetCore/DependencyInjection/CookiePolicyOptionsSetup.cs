using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class CookiePolicyOptionsSetup : IConfigureOptions<CookiePolicyOptions>
{
    public void Configure(CookiePolicyOptions cookiePolicyOptions)
        => cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
}
