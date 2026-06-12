using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

public sealed record StrategicOverviewModel(
    ProjectBoardResponse Board,
    IReadOnlyList<StrategicObjectiveSummaryModel> Objectives,
    IReadOnlyList<MilestoneTimelineItemModel> UpcomingMilestones,
    IReadOnlyList<OwnershipAllocationModel> OwnershipAllocation,
    DateTimeOffset LastUpdatedAt,
    bool IsPlaceholderData,
    string DataSourceLabel);

public sealed record StrategicObjectiveSummaryModel(
    Guid WorkItemId,
    string Title,
    string OwnerName,
    string StatusText,
    string StatusBadgeClass,
    string StatusIconName,
    int CompletionPercent,
    int InitiativeCount,
    int OpenMilestoneCount,
    string HealthSummaryText,
    string UpdatedBy,
    DateTimeOffset UpdatedAt,
    bool IsAtRisk,
    string DrillDownRoute);

public sealed record MilestoneTimelineItemModel(
    Guid WorkItemId,
    Guid ObjectiveWorkItemId,
    string ObjectiveTitle,
    string Label,
    string StatusText,
    string StatusBadgeClass,
    string StatusIconName,
    string CheckpointSummary,
    int CompletionPercent,
    string UpdatedBy,
    DateTimeOffset UpdatedAt,
    string DetailRoute);

public sealed record OwnershipAllocationModel(
    string Name,
    string Role,
    int ObjectiveCount,
    int AtRiskCount,
    string ContactLabel);

public sealed record StrategicDashboardSummaryModel(
    DashboardSummaryCardModel StrategicHealthCard,
    DashboardSummaryCardModel ObjectivesAtRiskCard,
    DashboardSummaryCardModel MilestonesDueSoonCard,
    DateTimeOffset LastUpdatedAt,
    bool IsPlaceholderData,
    string DataSourceLabel);
