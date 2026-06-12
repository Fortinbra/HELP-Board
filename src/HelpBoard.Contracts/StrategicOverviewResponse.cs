namespace HelpBoard.Contracts;

/// <summary>Represents a project board in the shared board/work-item model.</summary>
public sealed record ProjectBoardResponse(
    Guid Id,
    string Key,
    string Name,
    bool IsSystemBoard,
    bool IsDeletable,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<BoardWorkItemSummaryResponse> WorkItems);

/// <summary>Represents a work item summary on a project board.</summary>
public sealed record BoardWorkItemSummaryResponse(
    Guid Id,
    Guid? ParentWorkItemId,
    string Type,
    string Title,
    string Status,
    decimal ProgressPercent,
    string Owner,
    string UpdatedBy,
    DateTimeOffset UpdatedAt);

/// <summary>Represents the strategic overview feed used by the dashboard and strategic plan page.</summary>
public sealed record StrategicOverviewResponse(
    IReadOnlyList<StrategicObjectiveOverviewResponse> Objectives,
    IReadOnlyList<StrategicMilestoneOverviewResponse> UpcomingMilestones,
    IReadOnlyList<StrategicOwnershipAllocationResponse> OwnershipAllocation,
    DateTimeOffset LastUpdatedAt,
    bool IsPlaceholderData,
    string DataSourceLabel);

/// <summary>Represents a strategic objective summary with deterministic ownership and update metadata.</summary>
public sealed record StrategicObjectiveOverviewResponse(
    string Id,
    string Title,
    string OwnerName,
    string StatusText,
    string StatusBadgeClass,
    string StatusIconName,
    int CompletionPercent,
    int RiskScore,
    int InitiativeCount,
    int OpenMilestoneCount,
    string DueDateText,
    string HealthSummaryText,
    bool IsAtRisk,
    string DrillDownRoute,
    string UpdatedBy,
    DateTimeOffset UpdatedAt);

/// <summary>Represents a milestone summary used by the strategic overview feed.</summary>
public sealed record StrategicMilestoneOverviewResponse(
    string Id,
    string ObjectiveId,
    string ObjectiveTitle,
    string Label,
    string StatusText,
    string StatusBadgeClass,
    string StatusIconName,
    DateOnly TargetDate,
    string CheckpointSummary,
    int CompletionPercent,
    bool IsDueSoon,
    bool IsOverdue,
    string DetailRoute,
    string UpdatedBy,
    DateTimeOffset UpdatedAt);

/// <summary>Represents the ownership allocation section of the strategic overview feed.</summary>
public sealed record StrategicOwnershipAllocationResponse(
    string Name,
    string Role,
    int ObjectiveCount,
    int AtRiskCount,
    string ContactLabel);
