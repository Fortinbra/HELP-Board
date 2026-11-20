using System.Net;
using System.Net.Http.Json;
using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace HelpBoard.Api.Tests.Controllers;

public sealed class DashboardsControllerTests
{
    [Fact]
    public async Task GetAll_Anonymous_ReturnsUnauthorized()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development"));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });

        using var response = await client.GetAsync("/api/dashboards");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAntiforgeryToken_ReturnsBadRequest()
    {
        using var factory = new HelpBoardWebApplicationFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        using var response = await client.PostAsJsonAsync("/api/dashboards", new CreateDashboardRequest { Name = "Test" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_InvalidOverlappingLayout_IsRejectedBeforeService()
    {
        var service = new Mock<IDashboardService>(MockBehavior.Strict);
        using var factory = new HelpBoardWebApplicationFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDashboardService>();
                services.AddSingleton(service.Object);
            }));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var token = await client.GetFromJsonAsync<Token>("/auth/antiforgery");
        var widget = new DashboardWidget(Guid.NewGuid(), "Text", "Notes", "", 0, 0, 2, 2);
        var update = new UpdateDashboardRequest
        {
            Name = "Test", Revision = Guid.NewGuid(),
            Widgets = [widget, widget with { Id = Guid.NewGuid(), Column = 1 }]
        };
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(update)
        };
        request.Headers.Add("X-CSRF-TOKEN", token!.TokenValue);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_StaleRevision_ReturnsConflict()
    {
        var service = new Mock<IDashboardService>();
        service.Setup(item => item.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateDashboardRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardUpdateResult(null, true));
        using var factory = new HelpBoardWebApplicationFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDashboardService>();
                services.AddSingleton(service.Object);
            }));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var token = await client.GetFromJsonAsync<Token>("/auth/antiforgery");
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/dashboards/{Guid.NewGuid()}")
        {
            Content = JsonContent.Create(new UpdateDashboardRequest { Name = "Test", Revision = Guid.NewGuid(), Widgets = [] })
        };
        request.Headers.Add("X-CSRF-TOKEN", token!.TokenValue);

        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndUpdate_TwoEditors_RoundTripsWidgetsAndRejectsStaleEdit()
    {
        var store = new DashboardStore();
        using var factory = new HelpBoardWebApplicationFactory().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IDashboardReader>();
                services.RemoveAll<IDashboardWriter>();
                services.AddSingleton<IDashboardReader>(store);
                services.AddSingleton<IDashboardWriter>(store);
            }));
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var token = await client.GetFromJsonAsync<Token>("/auth/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token!.TokenValue);
        using var createdResponse = await client.PostAsJsonAsync("/api/dashboards", new CreateDashboardRequest { Name = "Shared" });
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        Assert.NotNull(createdResponse.Headers.Location);
        var created = (await createdResponse.Content.ReadFromJsonAsync<DashboardResponse>())!;
        var widget = new DashboardWidget(Guid.NewGuid(), "Text", "Notes", "Hello", 0, 0, 2, 2);
        var update = new UpdateDashboardRequest { Name = created.Name, Revision = created.Revision, Widgets = [widget] };

        using var saved = await client.PutAsJsonAsync($"/api/dashboards/{created.Id}", update);
        using var stale = await client.PutAsJsonAsync($"/api/dashboards/{created.Id}", update);
        var reloaded = await client.GetFromJsonAsync<DashboardResponse>($"/api/dashboards/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(widget, Assert.Single(reloaded!.Widgets));
        Assert.NotEqual(created.Revision, reloaded.Revision);
    }

    private sealed class DashboardStore : IDashboardReader, IDashboardWriter
    {
        private readonly Dictionary<Guid, DashboardDocument> _items = [];

        public Task<IReadOnlyList<DashboardDocument>> GetAllAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DashboardDocument>>(_items.Values.Select(Copy).ToList());

        public Task<DashboardDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_items.TryGetValue(id, out var item) ? Copy(item) : null);

        public Task AddAsync(DashboardDocument dashboard, CancellationToken cancellationToken)
        {
            _items.Add(dashboard.Id, Copy(dashboard));
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(DashboardDocument dashboard, Guid expectedRevision, CancellationToken cancellationToken)
        {
            if (!_items.TryGetValue(dashboard.Id, out var item) || item.Revision != expectedRevision)
            {
                return Task.FromResult(false);
            }

            _items[dashboard.Id] = Copy(dashboard);
            return Task.FromResult(true);
        }

        private static DashboardDocument Copy(DashboardDocument item) => new()
        {
            Id = item.Id, Name = item.Name, Revision = item.Revision, WidgetsJson = item.WidgetsJson
        };
    }

    private sealed record Token([property: System.Text.Json.Serialization.JsonPropertyName("token")] string TokenValue);
}
