using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

internal class KestrelServerOptionsSetup : IConfigureOptions<KestrelServerOptions>
{
    public void Configure(KestrelServerOptions kestrelServerOptions)
        => kestrelServerOptions.AddServerHeader = false;
}
