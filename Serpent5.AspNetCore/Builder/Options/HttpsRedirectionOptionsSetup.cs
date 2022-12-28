using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class HttpsRedirectionOptionsSetup : IConfigureOptions<HttpsRedirectionOptions>
{
    // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl#configure-permanent-redirects-in-production
    public void Configure(HttpsRedirectionOptions httpsRedirectionOptions)
        => httpsRedirectionOptions.RedirectStatusCode = 301;
}
