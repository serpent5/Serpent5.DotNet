using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class ExceptionHandlerOptionsSetupTests
{
    [Fact]
    public async Task DoesNotChangeStatusCode()
    {
        var testHost = await StartTestHostWithExceptionAsync();

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.InternalServerError, httpResponseMessage.StatusCode);
    }

    [Fact]
    public async Task DoesNotProduceContent()
    {
        var testHost = await StartTestHostWithExceptionAsync();

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(0, httpResponseMessage.Content.Headers.ContentLength);
    }

    private static Task<IHost> StartTestHostWithExceptionAsync()
        => TestHostBuilder.StartAsync(
            applicationBuilder =>
            {

                applicationBuilder.UseExceptionHandler();
#pragma warning disable CA2201 // Do not raise reserved exception types
                applicationBuilder.Run(_ => throw new Exception(string.Empty));
#pragma warning restore CA2201 // Do not raise reserved exception types
            },
            serviceCollection => serviceCollection.AddTransient<IConfigureOptions<ExceptionHandlerOptions>, ExceptionHandlerOptionsSetup>());
}
