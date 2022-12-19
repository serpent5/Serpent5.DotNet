using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class CookiePolicyOptionsSetup : IConfigureOptions<CookiePolicyOptions>
{
    public void Configure(CookiePolicyOptions cookiePolicyOptions)
        => cookiePolicyOptions.Secure = CookieSecurePolicy.Always;
}
