using HelpBoard.Web.Layout;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpBoard.Web.Tests.Layout;

public sealed class LandingLayoutTests
{
    [Fact]
    public async Task Render_ShowsOnlyPublicBrandAndContentWithoutDashboardNavigation()
    {
        // Arrange
        var services = new ServiceCollection().AddLogging();
        var flags = new Mock<IFeatureToggleService>();
        flags.Setup(service => service.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(true);
        services.AddSingleton(flags.Object);
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
        Assert.Contains("value=\"geocities\"", html);
        Assert.Contains("value=\"minimalist\"", html);
        Assert.Contains("value=\"western\"", html);
        Assert.DoesNotContain("Reports &amp; planning", html);
        Assert.DoesNotContain("baseline-sidebar", html);
        Assert.DoesNotContain("app-sidebar", html);
    }
}
