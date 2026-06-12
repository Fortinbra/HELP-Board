using HelpBoard.Services.Strategic;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpBoard.Services.Tests.Strategic;

public sealed class StrategicPlanServiceTests
{
    private readonly StrategicPlanService _sut = new(NullLogger<StrategicPlanService>.Instance);

    [Fact]
    public async Task GetStrategicBoardAsync_RollsUpHierarchyProgressAndMetadata_Deterministically()
    {
        // Arrange

        // Act
        var board = await _sut.GetStrategicBoardAsync();

        // Assert
        Assert.True(board.IsSystemBoard);
        Assert.False(board.IsDeletable);
        Assert.Equal("strategic-plan", board.Key);

        var reliabilityObjective = Assert.Single(board.WorkItems, item => item.Id == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));
        Assert.Equal("AtRisk", reliabilityObjective.Status);
        Assert.Equal(65m, reliabilityObjective.ProgressPercent);
        Assert.Equal("strategy.pm", reliabilityObjective.UpdatedBy);
        Assert.Equal(new DateTimeOffset(2026, 6, 10, 11, 15, 0, TimeSpan.Zero), reliabilityObjective.UpdatedAt);

        var deliveryObjective = Assert.Single(board.WorkItems, item => item.Id == Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));
        Assert.Equal("Blocked", deliveryObjective.Status);
        Assert.Equal(10m, deliveryObjective.ProgressPercent);

        var platformTheme = Assert.Single(board.WorkItems, item => item.Id == Guid.Parse("11111111-1111-1111-1111-111111111111"));
        Assert.Equal("AtRisk", platformTheme.Status);
        Assert.Equal(65m, platformTheme.ProgressPercent);
    }

    [Fact]
    public async Task GetStrategicOverviewAsync_ReturnsStableOverviewFeed_WithMetadataAndRollupValues()
    {
        // Arrange

        // Act
        var overview = await _sut.GetStrategicOverviewAsync();

        // Assert
        Assert.False(overview.IsPlaceholderData);
        Assert.Equal("Strategic plan system board", overview.DataSourceLabel);
        Assert.Equal(new DateTimeOffset(2026, 6, 10, 14, 30, 0, TimeSpan.Zero), overview.LastUpdatedAt);
        Assert.Equal(2, overview.Objectives.Count);
        Assert.Equal(5, overview.UpcomingMilestones.Count);

        var reliabilityObjective = Assert.Single(overview.Objectives, item => item.Id == "objective-increase-service-reliability");
        Assert.Equal("Increase service reliability", reliabilityObjective.Title);
        Assert.Equal("At risk", reliabilityObjective.StatusText);
        Assert.Equal(65, reliabilityObjective.CompletionPercent);
        Assert.Equal(2, reliabilityObjective.InitiativeCount);
        Assert.Equal(3, reliabilityObjective.OpenMilestoneCount);
        Assert.Equal("strategy.pm", reliabilityObjective.UpdatedBy);
        Assert.Equal(new DateTimeOffset(2026, 6, 10, 11, 15, 0, TimeSpan.Zero), reliabilityObjective.UpdatedAt);

        var firstMilestone = Assert.Single(overview.UpcomingMilestones, item => item.Id == "30000000-0000-0000-0000-000000000001");
        Assert.Equal("objective-increase-service-reliability", firstMilestone.ObjectiveId);
        Assert.Equal("Distributed tracing coverage", firstMilestone.Label);
        Assert.True(firstMilestone.IsDueSoon);
        Assert.False(firstMilestone.IsOverdue);
        Assert.Equal("strategy.pm", firstMilestone.UpdatedBy);
    }

    [Fact]
    public async Task GetStrategicWorkItemDetailAsync_WithObjective_ReturnsDirectChildrenOnly()
    {
        // Arrange
        var objectiveId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Act
        var detail = await _sut.GetStrategicWorkItemDetailAsync(objectiveId);

        // Assert
        Assert.NotNull(detail);
        Assert.Equal(objectiveId, detail!.WorkItem.Id);
        Assert.All(detail.Children, child => Assert.Equal(objectiveId, child.ParentWorkItemId));
        Assert.Equal(2, detail.Children.Count);
    }
}