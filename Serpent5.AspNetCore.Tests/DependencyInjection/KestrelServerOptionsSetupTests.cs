using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class KestrelServerOptionsSetupTests
{
    [Fact]
    public void Does_Not_Add_Server_Header()
    {
        var kestrelOptionsSetup = new KestrelServerOptionsSetup();
        var kestrelServerOptions = new KestrelServerOptions();

        kestrelOptionsSetup.Configure(kestrelServerOptions);

        Assert.False(kestrelServerOptions.AddServerHeader);
    }
}
