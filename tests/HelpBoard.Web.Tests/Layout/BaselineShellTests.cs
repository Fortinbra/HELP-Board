using HelpBoard.Web.Layout;
using HelpBoard.Contracts;
using HelpBoard.Web.Components;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Web.Tests.Layout;

public sealed class BaselineShellTests
{
    [Fact]
    public async Task Render_Baseline_ShowsBrandThemeLoginAndDisabledReports()
    {
        var services = CreateServices(new(false, false, null, null, null));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<BaselineShell>();
            return result.ToHtmlString();
        });

        Assert.Contains("images/help-logo.png", html);
        Assert.Contains("Theme", html);
        Assert.Contains("Not signed in", html);
        Assert.Contains("Google Workspace sign-in is not configured.", html);
        Assert.Contains("Director's report", System.Net.WebUtility.HtmlDecode(html));
        Assert.Contains("Strategic plan", html);
        Assert.Contains("Budget report", html);
        Assert.Equal(3, html.Split("aria-disabled=\"true\"").Length - 1);
        Assert.DoesNotContain("href=\"strategic-plan\"", html);
        Assert.DoesNotContain("href=\"#\"", html);
        Assert.Contains("Skip to main content", html);
    }

    [Fact]
    public async Task Render_WithBody_PlacesContentInMainLandmark()
    {
        var services = CreateServices(new(false, false, null, null, null));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        RenderFragment body = builder => builder.AddContent(0, "Hello, HELP board!");

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<BaselineShell>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["ChildContent"] = body }));
            return result.ToHtmlString();
        });

        Assert.Contains("<main id=\"main-content\"", html);
        Assert.Contains("Hello, HELP board!", html);
    }

    [Fact]
    public async Task Render_DisabledHelpTheme_HidesOption()
    {
        var services = CreateServices(new(false, false, null, null, null));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<BaselineShell>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["IsHelpEnabled"] = false }));
            return result.ToHtmlString();
        });

        Assert.DoesNotContain("value=\"help\"", html);
        Assert.Contains("value=\"dark\"", html);
        Assert.Contains("value=\"light\"", html);
    }

    [Fact]
    public async Task Render_GoogleSignInEnabled_ShowsSignInLink()
    {
        var services = CreateServices(new(true, false, null, null, null));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<AuthenticationPanel>();
            return result.ToHtmlString();
        });

        Assert.Contains("href=\"/auth/sign-in\"", html);
        Assert.Contains("Sign in with Google Workspace", html);
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
