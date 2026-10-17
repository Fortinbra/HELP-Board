using HelpBoard.Web.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Web.Tests.Layout;

public sealed class LandingLayoutTests
{
    [Fact]
    public async Task Render_ShowsOnlyPublicBrandAndContentWithoutDashboardNavigation()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        RenderFragment body = builder => builder.AddContent(0, "Sign in with Google Workspace");

        // Act
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<LandingLayout>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { ["Body"] = body }));
            return result.ToHtmlString();
        });

        // Assert
        Assert.Contains("HELP-Board", html);
        Assert.Contains("Skip to main content", html);
        Assert.Contains("Sign in with Google Workspace", html);
        Assert.DoesNotContain("Reports &amp; planning", html);
        Assert.DoesNotContain("baseline-sidebar", html);
        Assert.DoesNotContain("app-sidebar", html);
    }
}
