using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class KestrelServerOptionsSetupTests
{
    [Fact]
    public void DisablesServerHeader()
    {
        var kestrelOptionsSetup = new KestrelServerOptionsSetup();
        var kestrelServerOptions = new KestrelServerOptions();

        kestrelOptionsSetup.Configure(kestrelServerOptions);

        Assert.False(kestrelServerOptions.AddServerHeader);
    }
}
