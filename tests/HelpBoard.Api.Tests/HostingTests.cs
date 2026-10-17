using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpBoard.Api.Tests;

/// <summary>Verifies the container probe and reverse-proxy trust boundary.</summary>
public sealed class HostingTests
{
    [Fact]
    public async Task HealthProbe_ProductionWithoutDatabase_ReturnsHealthy()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Production")
                .UseSetting("ConnectionStrings:HelpBoard", "Host=unreachable;Database=probe;Username=probe")
                .UseSetting("https_port", "443"));
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        using var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("10.0.0.10", StatusCodes.Status200OK)]
    [InlineData("10.0.0.11", StatusCodes.Status307TemporaryRedirect)]
    public async Task ForwardedHeaders_ProxyAddress_OnlyTrustsConfiguredProxy(string remoteAddress, int expectedStatus)
    {
        using var factory = new HelpBoardWebApplicationFactory()
            .WithWebHostBuilder(builder => builder
                .UseSetting("ReverseProxy:KnownProxy", "10.0.0.10")
                .UseSetting("https_port", "443"));
        _ = factory.Server;

        var context = await factory.Server.SendAsync(request =>
        {
            request.Connection.RemoteIpAddress = IPAddress.Parse(remoteAddress);
            request.Request.Scheme = "http";
            request.Request.Host = new HostString("localhost");
            request.Request.Path = "/auth/status";
            request.Request.Headers["X-Forwarded-Proto"] = "https";
            request.Request.Headers["X-Forwarded-For"] = "192.0.2.1";
        });

        Assert.Equal(expectedStatus, context.Response.StatusCode);
    }
}
