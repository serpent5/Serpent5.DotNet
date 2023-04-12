using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Serpent5.AspNetCore.Tests.DependencyInjection;

public class HealthCheckOptionsSetupTests
{
    [Fact]
    public async Task Disables_Default_Response_Content()
    {
        var testHost = await new TestHostBuilder()
            .ConfigureServices(static serviceCollection =>
            {
                serviceCollection.AddHealthChecks();
                serviceCollection.AddTransient<IConfigureOptions<HealthCheckOptions>, HealthCheckOptionsSetup>();
            })
            .Configure(static applicationBuilder =>
            {
                applicationBuilder.UseRouting();
                applicationBuilder.UseEndpoints(endpointRouteBuilder =>
                {
                    endpointRouteBuilder.MapHealthChecks("/");
                });
            })
            .StartAsync();

        using var httpResponseMessage = await testHost.GetTestClient().GetAsync(new Uri("/", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        Assert.Equal(0, httpResponseMessage.Content.Headers.ContentLength);
    }
}
