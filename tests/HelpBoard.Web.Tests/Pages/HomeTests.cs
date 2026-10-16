using HelpBoard.Web.Pages;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace HelpBoard.Web.Tests.Pages;

public sealed class HomeTests
{
    [Theory]
    [InlineData(true, "Hello, HELP board!", "Foundation cards")]
    [InlineData(false, "Foundation cards", "Hello, HELP board!")]
    public async Task Render_BaselineFlag_SelectsExpectedContentWithoutStrategicFetch(
        bool enabled, string expected, string absent)
    {
        var flags = new Mock<IFeatureToggleService>(MockBehavior.Strict);
        flags.Setup(service => service.IsEnabledAsync(FeatureToggleKeys.HelloWorldBaseline)).ReturnsAsync(enabled);
        if (!enabled)
        {
            flags.Setup(service => service.IsStrategicPlanEnabledAsync()).ReturnsAsync(false);
        }
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(flags.Object);
        services.AddSingleton<DashboardFoundationService>();
        services.AddSingleton(new StrategicPlanApiService(new HttpClient()));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<Home>();
            return result.ToHtmlString();
        });

        Assert.Contains(expected, html);
        Assert.DoesNotContain(absent, html);
        flags.VerifyAll();
        flags.VerifyNoOtherCalls();
    }
}
