using HelpBoard.Web.Layout;
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
        var services = new ServiceCollection().AddLogging();
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
        Assert.Contains("Sign-in is not available in this baseline", html);
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
        var services = new ServiceCollection().AddLogging();
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
        var services = new ServiceCollection().AddLogging();
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
}
