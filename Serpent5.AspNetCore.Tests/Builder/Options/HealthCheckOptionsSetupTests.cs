using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serpent5.AspNetCore.Builder.Options;

namespace Serpent5.AspNetCore.Tests.Builder.Options;

public class HealthCheckOptionsSetupTests
{
    [Fact]
    public async Task DisablesDefaultResponseContent()
    {
        var testHost = await TestHostBuilder.StartAsync(
            applicationBuilder =>
            {

                applicationBuilder.UseRouting();
                applicationBuilder.UseEndpoints(endpointRouteBuilder =>
                {
                    endpointRouteBuilder.MapHealthChecks("/");
                });
            },
            serviceCollection =>
            {
                serviceCollection.AddHealthChecks();
                serviceCollection.AddTransient<IConfigureOptions<HealthCheckOptions>, HealthCheckOptionsSetup>();
            });

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        Assert.Equal(0, httpResponseMessage.Content.Headers.ContentLength);
    }
}
