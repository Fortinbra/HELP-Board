using System.Net;
using System.Net.Http.Json;
using HelpBoard.Contracts;

namespace HelpBoard.Api.Tests.Controllers;

public sealed class StrategicControllerTests(HelpBoardWebApplicationFactory factory)
    : IClassFixture<HelpBoardWebApplicationFactory>
{
    private static readonly Guid ReliabilityObjectiveId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task GetBoard_ReturnsReservedStrategicBoardWithWorkItems()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/strategic/board");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ProjectBoardResponse>();
        Assert.NotNull(payload);

        Assert.Equal("strategic-plan", payload.Key);
        Assert.Equal("Strategic Plan", payload.Name);
        Assert.True(payload.IsSystemBoard);
        Assert.False(payload.IsDeletable);
        Assert.NotEmpty(payload.WorkItems);

        Assert.Contains(payload.WorkItems, item => item.Type == "objective" && item.ParentWorkItemId is not null);
        Assert.Contains(payload.WorkItems, item => item.Type == "initiative" && item.ParentWorkItemId is not null);
        Assert.Contains(payload.WorkItems, item => item.Type == "milestone" && item.ParentWorkItemId is not null);
    }

    [Fact]
    public async Task GetOverview_ReturnsStableStrategicOverviewFeed()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/strategic/overview");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<StrategicOverviewResponse>();
        Assert.NotNull(payload);

        Assert.False(payload.IsPlaceholderData);
        Assert.Equal("Strategic plan system board", payload.DataSourceLabel);
        Assert.NotEmpty(payload.Objectives);
        Assert.NotEmpty(payload.UpcomingMilestones);
        Assert.All(payload.Objectives, item => Assert.False(string.IsNullOrWhiteSpace(item.UpdatedBy)));
        Assert.All(payload.UpcomingMilestones, item => Assert.False(string.IsNullOrWhiteSpace(item.UpdatedBy)));
    }

    [Fact]
    public async Task GetWorkItem_WithKnownObjective_ReturnsChildren()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/strategic/work-items/{ReliabilityObjectiveId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<BoardWorkItemDetailResponse>();
        Assert.NotNull(payload);

        Assert.Equal(ReliabilityObjectiveId, payload.WorkItem.Id);
        Assert.Equal("objective", payload.WorkItem.Type);
        Assert.All(payload.Children, child => Assert.Equal(ReliabilityObjectiveId, child.ParentWorkItemId));
        Assert.NotEmpty(payload.Children);
    }

    [Fact]
    public async Task GetWorkItem_WithUnknownObjective_ReturnsNotFound()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/strategic/work-items/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
