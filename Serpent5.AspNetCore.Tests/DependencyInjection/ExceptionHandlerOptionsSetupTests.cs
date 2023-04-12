using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class ExceptionHandlerOptionsSetupTests
{
    [Fact]
    public async Task Does_Not_Change_StatusCode()
    {
        var testHost = await StartTestHostWithExceptionAsync();

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponseMessage.StatusCode);
    }

    [Fact]
    public async Task Does_Not_Produce_Content()
    {
        var testHost = await StartTestHostWithExceptionAsync();

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(0, httpResponseMessage.Content.Headers.ContentLength);
    }

    private static Task<IHost> StartTestHostWithExceptionAsync()
        => new TestHostBuilder()
            .ConfigureServices(static serviceCollection => serviceCollection.AddTransient<IConfigureOptions<ExceptionHandlerOptions>, ExceptionHandlerOptionsSetup>())
            .Configure(static applicationBuilder =>
            {
                applicationBuilder.UseExceptionHandler();
#pragma warning disable CA2201 // Do not raise reserved exception types
                applicationBuilder.Run(_ => throw new Exception(string.Empty));
#pragma warning restore CA2201 // Do not raise reserved exception types
            })
            .StartAsync();
}
