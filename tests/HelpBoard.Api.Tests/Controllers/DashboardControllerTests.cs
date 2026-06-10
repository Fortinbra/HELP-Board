using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using HelpBoard.Abstractions.Domain;
using Microsoft.Extensions.DependencyInjection;
using HelpBoard.Contracts;

namespace HelpBoard.Api.Tests.Controllers;

public sealed class DashboardControllerTests(HelpBoardWebApplicationFactory factory)
    : IClassFixture<HelpBoardWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task GetSummaryAsync_WhenNoTicketsExist_ReturnsOkWithZeroCounts()
    {
        // Arrange
        var client = factory.CreateClient();
        ResetStore();

        // Act
        var response = await client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>(JsonOptions);
        Assert.NotNull(body);
        var summary = body;

        Assert.Equal(0, summary.TotalTickets);
        Assert.Equal(0, summary.OpenTickets);
        Assert.Equal(0, summary.ClosedTickets);
        Assert.Equal(Enum.GetValues<TicketStatus>().Length, summary.StatusCounts.Count);
        Assert.All(summary.StatusCounts, statusCount => Assert.Equal(0, statusCount.Count));
        Assert.True(summary.LastUpdatedUtc > DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task GetSummaryAsync_WhenTicketsExist_ReturnsAggregatedStatusCounts()
    {
        // Arrange
        var client = factory.CreateClient();
        ResetStore();
        await SeedTicketsAsync(
        [
            BuildTicket("Open ticket", TicketStatus.Open),
            BuildTicket("In progress ticket", TicketStatus.InProgress),
            BuildTicket("Closed ticket", TicketStatus.Closed)
        ]);

        // Act
        var response = await client.GetAsync("/api/dashboard/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>(JsonOptions);
        Assert.NotNull(body);
        var summary = body;

        Assert.Equal(3, summary.TotalTickets);
        Assert.Equal(2, summary.OpenTickets);
        Assert.Equal(1, summary.ClosedTickets);

        Assert.Equal(1, GetStatusCount(summary.StatusCounts, TicketStatus.Open));
        Assert.Equal(1, GetStatusCount(summary.StatusCounts, TicketStatus.InProgress));
        Assert.Equal(0, GetStatusCount(summary.StatusCounts, TicketStatus.Resolved));
        Assert.Equal(1, GetStatusCount(summary.StatusCounts, TicketStatus.Closed));
    }

    private void ResetStore()
    {
        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<HelpBoardWebApplicationFactory.ITestTicketStore>();
        store.Reset();
    }

    private async Task SeedTicketsAsync(IEnumerable<Ticket> tickets)
    {
        using var scope = factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<HelpBoardWebApplicationFactory.ITestTicketStore>();
        await store.SeedAsync(tickets);
    }

    private static int GetStatusCount(IReadOnlyList<DashboardStatusCountResponse> statusCounts, TicketStatus status)
        => statusCounts.Single(x => x.Status == status).Count;

    private static Ticket BuildTicket(string title, TicketStatus status) =>
        new()
        {
            Title = title,
            Description = "Dashboard seeded test ticket",
            CreatedBy = "dashboard@test.local",
            Priority = TicketPriority.Medium,
            Status = status,
            UpdatedAt = DateTimeOffset.UtcNow
        };
}