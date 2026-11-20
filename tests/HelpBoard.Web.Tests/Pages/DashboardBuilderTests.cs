using System.Net;
using System.Net.Http.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Components;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Web.Tests.Pages;

public sealed class DashboardBuilderTests
{
    [Fact]
    public async Task Render_DataAndTextWidgets_ShowsUnconfiguredStateAndSavedNotes()
    {
        var widgets = new DashboardWidget[]
        {
            new(Guid.NewGuid(), "Line", "Trend", "", 0, 0, 2, 2),
            new(Guid.NewGuid(), "Text", "Notes", "Remember this", 2, 0, 2, 2)
        };
        var dashboard = new DashboardResponse(Guid.NewGuid(), "Board", Guid.NewGuid(), widgets);
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton(new DashboardApiClient(new HttpClient(new DashboardHandler(dashboard))
        {
            BaseAddress = new Uri("https://localhost/")
        }));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());

        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<DashboardBuilder>();
            return result.ToHtmlString();
        });

        Assert.Contains("No data source configured", html);
        Assert.Contains("Remember this", html);
        Assert.Contains("Move Notes", html);
        Assert.Contains("Resize Notes", html);
    }

    private sealed class DashboardHandler(DashboardResponse dashboard) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[] { dashboard })
            });
    }
}
