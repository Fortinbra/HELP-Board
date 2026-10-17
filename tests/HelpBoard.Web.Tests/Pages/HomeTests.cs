using HelpBoard.Contracts;
using HelpBoard.Web.Pages;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Web.Tests.Pages;

public sealed class HomeTests
{
    [Fact]
    public async Task Render_AnonymousVisitor_ShowsPublicLandingAndWorkspaceSignIn()
    {
        // Arrange
        var services = CreateServices(new(true, false, null, null, null));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<Home>();
            return result.ToHtmlString();
        });

        // Assert
        Assert.Contains("Welcome to HELP-Board", html);
        Assert.Contains("Sign in with Google Workspace", html);
        Assert.Contains("href=\"/auth/sign-in?returnUrl=%2Fdashboard\"", html);
        Assert.DoesNotContain("Foundation cards", html);
        Assert.DoesNotContain("Strategic plan", html);
        Assert.DoesNotContain("Clearer insight", html);
    }

    [Fact]
    public async Task Render_AuthenticatedVisitor_ShowsDashboardLink()
    {
        // Arrange
        var services = CreateServices(new(true, true, "Board Member", "member@example.org", "Viewer"));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<Home>();
            return result.ToHtmlString();
        });

        // Assert
        Assert.Contains("Welcome to HELP-Board", html);
        Assert.Contains("href=\"/dashboard\"", html);
        Assert.Contains("Continue to the dashboard", html);
        Assert.DoesNotContain("Sign in with Google Workspace", html);
    }

    private static IServiceCollection CreateServices(AuthenticationStatusResponse status)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IAuthenticationClient>(new FixedAuthenticationClient(status));
        services.AddSingleton<NavigationManager>(new TestNavigationManager());
        return services;
    }

    private sealed class FixedAuthenticationClient(AuthenticationStatusResponse status) : IAuthenticationClient
    {
        public Task<AuthenticationStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
            => Task.FromResult(status);

        public Task SignOutAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://localhost/", "https://localhost/");
        }

        protected override void NavigateToCore(string uri, NavigationOptions options)
        {
        }
    }
}
