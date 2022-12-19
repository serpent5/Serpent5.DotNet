using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Builder.Options;

internal class KestrelServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    public void Configure(KestrelServerOptions kestrelServerOptions)
        => kestrelServerOptions.AddServerHeader = false;
}
