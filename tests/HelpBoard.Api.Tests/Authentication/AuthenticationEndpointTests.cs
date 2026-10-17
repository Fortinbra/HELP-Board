using System.Net;
using System.Net.Http.Json;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HelpBoard.Api.Tests.Authentication;

public sealed class AuthenticationEndpointTests
{
    [Fact]
    public async Task GetStatus_WithoutGoogleConfiguration_ReturnsAnonymousStatus()
    {
        // Arrange
        using var factory = CreateUnauthenticatedFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/auth/status");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var status = await response.Content.ReadFromJsonAsync<AuthenticationStatusResponse>();
        Assert.NotNull(status);
        Assert.False(status.GoogleSignInEnabled);
        Assert.False(status.IsAuthenticated);
    }

    [Fact]
    public async Task GetTicketApi_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = CreateUnauthenticatedFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Act
        using var response = await client.GetAsync("/api/tickets");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSignIn_WithoutGoogleConfiguration_ReturnsServiceUnavailable()
    {
        // Arrange
        using var factory = CreateUnauthenticatedFactory();
        using var client = factory.CreateClient();

        // Act
        using var response = await client.GetAsync("/auth/sign-in");

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task PostSignOut_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        // Arrange
        using var factory = new HelpBoardWebApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.PostAsync("/auth/sign-out", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostSignOut_WithAntiforgeryToken_EndsSession()
    {
        // Arrange
        using var factory = new HelpBoardWebApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        using var tokenResponse = await client.GetAsync("/auth/antiforgery");
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<AntiforgeryTokenResponse>();
        Assert.NotNull(token);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/sign-out");
        request.Headers.Add("X-CSRF-TOKEN", token.Token);

        // Act
        using var response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetSignIn_WithGoogleConfiguration_StartsGoogleChallenge()
    {
        // Arrange
        using var factory = CreateGoogleConfiguredFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Act
        using var response = await client.GetAsync("/auth/sign-in");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("https://accounts.google.com/", response.Headers.Location?.AbsoluteUri);
    }

    [Fact]
    public async Task GetDashboard_WithoutAuthenticationOrProvider_RedirectsToPublicLanding()
    {
        // Arrange
        using var factory = CreateUnauthenticatedFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GetDashboard_WithoutAuthenticationWithProvider_RedirectsToSignIn()
    {
        // Arrange
        using var factory = CreateGoogleConfiguredFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(
            "/auth/sign-in?returnUrl=%2Fdashboard",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task GetDashboard_WithAuthenticatedSession_ReturnsApplicationShell()
    {
        // Arrange
        using var factory = new HelpBoardWebApplicationFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/dashboard");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"app\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetLanding_WithoutAuthentication_ReturnsPublicApplicationShell()
    {
        // Arrange
        using var factory = CreateUnauthenticatedFactory();
        using var client = factory.CreateClient(new()
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        using var response = await client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"app\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetSignIn_WithExternalReturnUrl_UsesSafeDefaultAfterGoogleChallenge()
    {
        // Arrange
        using var factory = CreateGoogleConfiguredFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Act
        using var response = await client.GetAsync(
            "/auth/sign-in?returnUrl=https%3A%2F%2Fevil.example");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("redirect_uri=", response.Headers.Location?.Query);
    }

    private static WebApplicationFactory<Program> CreateUnauthenticatedFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Development")
                .UseSetting("ConnectionStrings:HelpBoard", "Host=unreachable;Database=auth;Username=test")
                .UseSetting("Authentication:Google:ClientId", string.Empty)
                .UseSetting("Authentication:Google:ClientSecret", string.Empty)
                .UseSetting("Authentication:Google:WorkspaceDomain", string.Empty));
    }

    private static WebApplicationFactory<Program> CreateGoogleConfiguredFactory()
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder
                .UseEnvironment("Development")
                .UseSetting("ConnectionStrings:HelpBoard", "Host=unreachable;Database=auth;Username=test")
                .UseSetting("Authentication:Google:ClientId", "test-client-id")
                .UseSetting("Authentication:Google:ClientSecret", "test-client-secret")
                .UseSetting("Authentication:Google:WorkspaceDomain", "example.org"));
    }

    private sealed record AntiforgeryTokenResponse(string Token);
}
