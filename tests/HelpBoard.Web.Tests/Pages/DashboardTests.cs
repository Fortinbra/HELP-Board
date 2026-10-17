using HelpBoard.Contracts;
using HelpBoard.Web.Pages;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpBoard.Web.Tests.Pages;

public sealed class DashboardTests
{
    [Fact]
    public async Task Render_AnonymousVisitor_ShowsSignInAndDoesNotLoadDashboard()
    {
        // Arrange
        var flags = new Mock<IFeatureToggleService>(MockBehavior.Strict);
        var services = CreateServices(new(true, false, null, null, null), flags.Object);
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<Dashboard>();
            return result.ToHtmlString();
        });

        // Assert
        Assert.Contains("Sign in to view the dashboard", html);
        Assert.Contains("href=\"/auth/sign-in?returnUrl=%2Fdashboard\"", html);
        Assert.DoesNotContain("Foundation cards", html);
        flags.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Render_AuthenticatedVisitorWithBaselineEnabled_ShowsDashboard()
    {
        // Arrange
        var flags = new Mock<IFeatureToggleService>(MockBehavior.Strict);
        flags.Setup(service => service.IsEnabledAsync(FeatureToggleKeys.HelloWorldBaseline))
            .ReturnsAsync(true);
        var services = CreateServices(new(true, true, "Board Member", "member@example.org", "Viewer"), flags.Object);
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<Dashboard>();
            return result.ToHtmlString();
        });

        // Assert
        Assert.Contains("Hello, HELP board!", html);
        Assert.DoesNotContain("Sign in to view the dashboard", html);
        flags.VerifyAll();
        flags.VerifyNoOtherCalls();
    }

    private static IServiceCollection CreateServices(
        AuthenticationStatusResponse status,
        IFeatureToggleService featureToggleService)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<IAuthenticationClient>(new FixedAuthenticationClient(status));
        services.AddSingleton<NavigationManager>(new TestNavigationManager());
        services.AddSingleton(featureToggleService);
        services.AddSingleton(new DashboardFoundationService());
        services.AddSingleton(new StrategicPlanApiService(new HttpClient()));
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
