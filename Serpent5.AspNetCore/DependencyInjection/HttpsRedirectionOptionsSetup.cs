using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class HttpsRedirectionOptionsSetup : IConfigureOptions<HttpsRedirectionOptions>
{
    // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl#configure-permanent-redirects-in-production
    public void Configure(HttpsRedirectionOptions httpsRedirectionOptions)
        => httpsRedirectionOptions.RedirectStatusCode = 301;
}
