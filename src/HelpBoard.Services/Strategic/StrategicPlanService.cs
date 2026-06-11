using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Services.Strategic;

/// <summary>Provides the reserved Strategic Plan board using the shared board/work-item model.</summary>
public sealed class StrategicPlanService(ILogger<StrategicPlanService> logger) : IStrategicPlanService
{
    public static readonly Guid StrategicBoardId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public const string StrategicBoardKey = "strategic-plan";
    public const string StrategicBoardName = "Strategic Plan";

    private static readonly DateTimeOffset SnapshotAt = new(2026, 6, 11, 0, 0, 0, TimeSpan.Zero);

    private static readonly Guid ThemePlatformExcellenceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ThemeDeliveryAccelerationId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid ObjectiveReliabilityId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid ObjectiveLeadTimeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid InitiativeObservabilityId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid InitiativeIncidentResponseId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid InitiativeCiPipelineId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid MilestoneTracingId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid MilestoneSloDashboardId = Guid.Parse("30000000-0000-0000-0000-000000000002");
    private static readonly Guid MilestoneRunbookTrainingId = Guid.Parse("30000000-0000-0000-0000-000000000003");
    private static readonly Guid MilestoneEscalationPolicyId = Guid.Parse("30000000-0000-0000-0000-000000000004");
    private static readonly Guid MilestoneBuildCacheId = Guid.Parse("30000000-0000-0000-0000-000000000005");
    private static readonly Guid MilestoneParallelTestsId = Guid.Parse("30000000-0000-0000-0000-000000000006");

    private static readonly IReadOnlyList<StrategicWorkItemDefinition> Definitions = CreateDefinitions();
    private static readonly IReadOnlyDictionary<Guid, StrategicWorkItemDefinition> DefinitionsById = Definitions.ToDictionary(definition => definition.Id);

    public Task<ProjectBoardResponse> GetStrategicBoardAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = BuildState();

        logger.LogInformation("Returning strategic board with {WorkItemCount} work items", state.WorkItems.Count);

        return Task.FromResult(new ProjectBoardResponse(
            StrategicBoardId,
            StrategicBoardKey,
            StrategicBoardName,
            IsSystemBoard: true,
            IsDeletable: false,
            state.BoardUpdatedAt,
            state.WorkItems));
    }

    public Task<StrategicOverviewResponse> GetStrategicOverviewAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = BuildState();
        var overview = new StrategicOverviewResponse(
            BuildObjectiveOverview(state),
            BuildMilestoneOverview(state),
            BuildOwnershipAllocation(state),
            state.BoardUpdatedAt,
            IsPlaceholderData: false,
            DataSourceLabel: "Strategic plan system board");

        logger.LogInformation(
            "Returning strategic overview with {ObjectiveCount} objectives and {MilestoneCount} milestones",
            overview.Objectives.Count,
            overview.UpcomingMilestones.Count);

        return Task.FromResult(overview);
    }

    public Task<BoardWorkItemDetailResponse?> GetStrategicWorkItemDetailAsync(Guid workItemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var state = BuildState();
        var workItem = state.WorkItems.SingleOrDefault(item => item.Id == workItemId);

        if (workItem is null)
        {
            logger.LogInformation("Strategic work item {WorkItemId} was not found", workItemId);
            return Task.FromResult<BoardWorkItemDetailResponse?>(null);
        }

        var children = state.WorkItems.Where(item => item.ParentWorkItemId == workItemId).ToArray();
        return Task.FromResult<BoardWorkItemDetailResponse?>(new BoardWorkItemDetailResponse(workItem, children));
    }

    private static StrategicPlanState BuildState()
    {
        var definitionsByParent = Definitions
            .GroupBy(definition => definition.ParentWorkItemId ?? Guid.Empty)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(definition => GetTypeSortOrder(definition.Type))
                    .ThenBy(definition => definition.Title, StringComparer.Ordinal)
                    .ThenBy(definition => definition.Id)
                    .ToArray());

        var snapshots = new Dictionary<Guid, StrategicWorkItemSnapshot>();

        foreach (var definition in Definitions.OrderBy(definition => GetTypeSortOrder(definition.Type)).ThenBy(definition => definition.Title, StringComparer.Ordinal))
        {
            BuildSnapshot(definition.Id, definitionsByParent, snapshots);
        }

        var workItems = Definitions.Select(definition => snapshots[definition.Id].ToSummary()).ToArray();

        return new StrategicPlanState(snapshots, DefinitionsById, workItems, workItems.Max(item => item.UpdatedAt));
    }

    private static StrategicWorkItemSnapshot BuildSnapshot(
        Guid workItemId,
        IReadOnlyDictionary<Guid, StrategicWorkItemDefinition[]> definitionsByParent,
        IDictionary<Guid, StrategicWorkItemSnapshot> snapshots)
    {
        if (snapshots.TryGetValue(workItemId, out var existingSnapshot))
        {
            return existingSnapshot;
        }

        var definition = DefinitionsById[workItemId];
        var childDefinitions = definitionsByParent.TryGetValue(workItemId, out var children) ? children : [];
        var childSnapshots = childDefinitions.Select(childDefinition => BuildSnapshot(childDefinition.Id, definitionsByParent, snapshots)).ToArray();

        var snapshot = definition.SeedProgressPercent is not null
            ? new StrategicWorkItemSnapshot(definition, definition.SeedStatus ?? "Planned", definition.SeedProgressPercent.Value, definition.UpdatedBy, definition.UpdatedAt)
            : CreateRollupSnapshot(definition, childSnapshots);

        snapshots[workItemId] = snapshot;
        return snapshot;
    }

    private static StrategicWorkItemSnapshot CreateRollupSnapshot(
        StrategicWorkItemDefinition definition,
        IReadOnlyList<StrategicWorkItemSnapshot> childSnapshots)
    {
        if (childSnapshots.Count == 0)
        {
            return new StrategicWorkItemSnapshot(definition, "Planned", 0m, definition.UpdatedBy, definition.UpdatedAt);
        }

        var progressPercent = CalculateAverageProgress(childSnapshots);
        var status = DetermineRollupStatus(childSnapshots, progressPercent);
        var updatedAt = childSnapshots.Max(snapshot => snapshot.UpdatedAt);
        var updatedBy = SelectLatestUpdatedBy(childSnapshots, updatedAt);

        return new StrategicWorkItemSnapshot(definition, status, progressPercent, updatedBy, updatedAt);
    }

    private static decimal CalculateAverageProgress(IReadOnlyList<StrategicWorkItemSnapshot> childSnapshots)
        => Math.Round(childSnapshots.Average(snapshot => snapshot.ProgressPercent), 0, MidpointRounding.AwayFromZero);

    private static string DetermineRollupStatus(IReadOnlyList<StrategicWorkItemSnapshot> childSnapshots, decimal progressPercent)
    {
        if (childSnapshots.All(snapshot => snapshot.Status == "Completed"))
        {
            return "Completed";
        }

        if (childSnapshots.Any(snapshot => snapshot.Status == "Blocked"))
        {
            return "Blocked";
        }

        if (childSnapshots.Any(snapshot => snapshot.Status == "AtRisk"))
        {
            return "AtRisk";
        }

        if (progressPercent >= 70m)
        {
            return "OnTrack";
        }

        if (progressPercent >= 40m)
        {
            return "AtRisk";
        }

        return "Planned";
    }

    private static string SelectLatestUpdatedBy(IReadOnlyList<StrategicWorkItemSnapshot> childSnapshots, DateTimeOffset latestUpdatedAt)
        => childSnapshots
            .Where(snapshot => snapshot.UpdatedAt == latestUpdatedAt)
            .OrderBy(snapshot => snapshot.Definition.Title, StringComparer.Ordinal)
            .ThenBy(snapshot => snapshot.Definition.Id)
            .First()
            .UpdatedBy;

    private static IReadOnlyList<StrategicObjectiveOverviewResponse> BuildObjectiveOverview(StrategicPlanState state)
        => state.WorkItems
            .Where(item => item.Type == "objective")
            .Select(item => BuildObjectiveOverviewItem(state, item))
            .ToArray();

    private static StrategicObjectiveOverviewResponse BuildObjectiveOverviewItem(StrategicPlanState state, BoardWorkItemSummaryResponse item)
    {
        var childInitiatives = GetDirectChildren(state, item.Id, "initiative");
        var descendantMilestones = childInitiatives.SelectMany(initiative => GetDirectChildren(state, initiative.Id, "milestone")).ToArray();
        var latestDueDate = descendantMilestones
            .Select(milestone => state.DefinitionsById[milestone.Id].TargetDate)
            .Where(targetDate => targetDate is not null)
            .Select(targetDate => targetDate!.Value)
            .OrderBy(targetDate => targetDate)
            .LastOrDefault();

        return new StrategicObjectiveOverviewResponse(
            CreateObjectiveId(item.Title),
            item.Title,
            item.Owner,
            ToDisplayStatusText(item.Status),
            GetStatusBadgeClass(item.Status),
            GetStatusIconName(item.Status),
            (int)item.ProgressPercent,
            CalculateRiskScore(item.Status, descendantMilestones),
            childInitiatives.Count,
            descendantMilestones.Count(milestone => milestone.Status != "Completed"),
            latestDueDate == default ? "Not scheduled" : $"Due {latestDueDate:yyyy-MM-dd}",
            BuildObjectiveHealthSummary(item.Status, childInitiatives.Count, descendantMilestones),
            item.Status is "AtRisk" or "Blocked",
            CreateDetailRoute(item.Title),
            item.UpdatedBy,
            item.UpdatedAt);
    }

    private static IReadOnlyList<StrategicMilestoneOverviewResponse> BuildMilestoneOverview(StrategicPlanState state)
        => state.WorkItems
            .Where(item => item.Type == "milestone")
            .Where(item => item.Status != "Completed")
            .Select(item => BuildMilestoneOverviewItem(state, item))
            .OrderBy(item => item.TargetDate)
            .ThenBy(item => item.Label, StringComparer.Ordinal)
            .ToArray();

    private static StrategicMilestoneOverviewResponse BuildMilestoneOverviewItem(StrategicPlanState state, BoardWorkItemSummaryResponse item)
    {
        var initiative = state.WorkItems.Single(summary => summary.Id == item.ParentWorkItemId);
        var objective = state.WorkItems.Single(summary => summary.Id == initiative.ParentWorkItemId);
        var snapshotDate = DateOnly.FromDateTime(SnapshotAt.UtcDateTime);
        var targetDate = state.DefinitionsById[item.Id].TargetDate ?? snapshotDate;
        var isDueSoon = targetDate >= snapshotDate && targetDate <= snapshotDate.AddDays(14);

        return new StrategicMilestoneOverviewResponse(
            item.Id.ToString(),
            CreateObjectiveId(objective.Title),
            objective.Title,
            item.Title,
            ToDisplayStatusText(item.Status),
            GetStatusBadgeClass(item.Status),
            GetStatusIconName(item.Status),
            targetDate,
            BuildMilestoneCheckpointSummary(item.Status),
            (int)item.ProgressPercent,
            isDueSoon,
                targetDate < snapshotDate,
            CreateDetailRoute(objective.Title),
            item.UpdatedBy,
            item.UpdatedAt);
    }

    private static IReadOnlyList<StrategicOwnershipAllocationResponse> BuildOwnershipAllocation(StrategicPlanState state)
        => state.WorkItems
            .Where(item => item.Type == "objective")
            .GroupBy(item => item.Owner)
            .Select(group => new StrategicOwnershipAllocationResponse(
                group.Key,
                "Objective owner",
                group.Count(),
                group.Count(item => item.Status is "AtRisk" or "Blocked"),
                CreateContactLabel(group.Key)))
            .OrderBy(item => item.Name, StringComparer.Ordinal)
            .ToArray();

    private static IReadOnlyList<BoardWorkItemSummaryResponse> GetDirectChildren(StrategicPlanState state, Guid parentId, string type)
        => state.WorkItems.Where(item => item.ParentWorkItemId == parentId && item.Type == type).ToArray();

    private static int CalculateRiskScore(string status, IReadOnlyList<BoardWorkItemSummaryResponse> descendantMilestones)
    {
        var baseScore = status switch
        {
            "Blocked" => 8,
            "AtRisk" => 5,
            "OnTrack" => 2,
            "Completed" => 0,
            _ => 1
        };

        var atRiskCount = descendantMilestones.Count(item => item.Status == "AtRisk");
        var blockedCount = descendantMilestones.Count(item => item.Status == "Blocked");

        return Math.Clamp(baseScore + atRiskCount + blockedCount * 2, 0, 10);
    }

    private static string BuildObjectiveHealthSummary(string status, int initiativeCount, IReadOnlyList<BoardWorkItemSummaryResponse> descendantMilestones)
        => status switch
        {
            "Blocked" => $"{descendantMilestones.Count(item => item.Status == "Blocked")} blocked milestone(s) affect {initiativeCount} initiative(s).",
            "AtRisk" => $"{descendantMilestones.Count(item => item.Status == "AtRisk")} milestone(s) need attention across {initiativeCount} initiative(s).",
            "OnTrack" => $"{initiativeCount} initiative(s) are moving forward with predictable milestone delivery.",
            "Completed" => "All tracked milestone work is complete.",
            _ => "Strategic work is being planned."
        };

    private static string BuildMilestoneCheckpointSummary(string status)
        => status switch
        {
            "Blocked" => "Blocked checkpoint; dependency resolution is required.",
            "AtRisk" => "Checkpoint is slipping and needs owner intervention.",
            "OnTrack" => "Checkpoint remains on schedule.",
            "Completed" => "Checkpoint is complete.",
            _ => "Checkpoint is planned."
        };

    private static string ToDisplayStatusText(string status)
        => status switch
        {
            "Blocked" => "Blocked",
            "AtRisk" => "At risk",
            "OnTrack" => "On track",
            "Completed" => "Completed",
            _ => "Planned"
        };

    private static string GetStatusBadgeClass(string status)
        => status switch
        {
            "Blocked" => "hb-badge--danger",
            "AtRisk" => "hb-badge--warning",
            "OnTrack" => "hb-badge--success",
            "Completed" => "hb-badge--success",
            _ => "hb-badge--neutral"
        };

    private static string GetStatusIconName(string status)
        => status switch
        {
            "Blocked" => "ban",
            "AtRisk" => "triangle-alert",
            "OnTrack" => "check-circle-2",
            "Completed" => "badge-check",
            _ => "clock-3"
        };

    private static string CreateObjectiveId(string title)
        => $"objective-{CreateSlug(title)}";

    private static string CreateDetailRoute(string title)
    {
        var objectiveId = CreateObjectiveId(title);
        return $"strategic-plan/objectives/{objectiveId}#{objectiveId}";
    }

    private static string CreateContactLabel(string owner)
        => $"{CreateSlug(owner)}@helpboard.local";

    private static string CreateSlug(string value)
    {
        var normalized = new string(value
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return normalized.Trim('-');
    }

    private static int GetTypeSortOrder(string type)
        => type switch
        {
            "theme" => 0,
            "objective" => 1,
            "initiative" => 2,
            "milestone" => 3,
            _ => 4
        };

    private static IReadOnlyList<StrategicWorkItemDefinition> CreateDefinitions() =>
    [
        new StrategicWorkItemDefinition(ThemePlatformExcellenceId, null, "theme", "Platform Excellence", "CTO", "cto", new DateTimeOffset(2026, 6, 3, 9, 0, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(ThemeDeliveryAccelerationId, null, "theme", "Delivery Acceleration", "CTO", "cto", new DateTimeOffset(2026, 6, 3, 9, 0, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(ObjectiveReliabilityId, ThemePlatformExcellenceId, "objective", "Increase service reliability", "Platform Director", "platform.director", new DateTimeOffset(2026, 6, 6, 14, 30, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(ObjectiveLeadTimeId, ThemeDeliveryAccelerationId, "objective", "Reduce engineering lead time", "Engineering Director", "engineering.director", new DateTimeOffset(2026, 6, 7, 10, 15, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(InitiativeObservabilityId, ObjectiveReliabilityId, "initiative", "Improve observability coverage", "Platform Team", "platform.lead", new DateTimeOffset(2026, 6, 8, 8, 45, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(InitiativeIncidentResponseId, ObjectiveReliabilityId, "initiative", "Reduce mean time to recovery", "SRE Team", "sre.manager", new DateTimeOffset(2026, 6, 8, 12, 15, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(InitiativeCiPipelineId, ObjectiveLeadTimeId, "initiative", "Shorten CI cycle time", "Developer Experience", "devex.lead", new DateTimeOffset(2026, 6, 9, 11, 0, 0, TimeSpan.Zero)),
        new StrategicWorkItemDefinition(MilestoneTracingId, InitiativeObservabilityId, "milestone", "Distributed tracing coverage", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 8, 0, 0, TimeSpan.Zero), "OnTrack", 60m, new DateOnly(2026, 6, 20)),
        new StrategicWorkItemDefinition(MilestoneSloDashboardId, InitiativeObservabilityId, "milestone", "SLO dashboard published", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 9, 30, 0, TimeSpan.Zero), "AtRisk", 40m, new DateOnly(2026, 6, 28)),
        new StrategicWorkItemDefinition(MilestoneRunbookTrainingId, InitiativeIncidentResponseId, "milestone", "Runbook training completed", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 10, 0, 0, TimeSpan.Zero), "Completed", 100m, new DateOnly(2026, 6, 18)),
        new StrategicWorkItemDefinition(MilestoneEscalationPolicyId, InitiativeIncidentResponseId, "milestone", "Escalation policy finalized", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 11, 15, 0, TimeSpan.Zero), "OnTrack", 60m, new DateOnly(2026, 6, 24)),
        new StrategicWorkItemDefinition(MilestoneBuildCacheId, InitiativeCiPipelineId, "milestone", "Build cache optimization", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 13, 0, 0, TimeSpan.Zero), "Blocked", 20m, new DateOnly(2026, 7, 1)),
        new StrategicWorkItemDefinition(MilestoneParallelTestsId, InitiativeCiPipelineId, "milestone", "Parallel test execution", "Workstream Owner", "strategy.pm", new DateTimeOffset(2026, 6, 10, 14, 30, 0, TimeSpan.Zero), "Planned", 0m, new DateOnly(2026, 7, 6))
    ];

    private sealed record StrategicPlanState(
        IReadOnlyDictionary<Guid, StrategicWorkItemSnapshot> Snapshots,
        IReadOnlyDictionary<Guid, StrategicWorkItemDefinition> DefinitionsById,
        IReadOnlyList<BoardWorkItemSummaryResponse> WorkItems,
        DateTimeOffset BoardUpdatedAt)
    {
    }

    private sealed record StrategicWorkItemDefinition(
        Guid Id,
        Guid? ParentWorkItemId,
        string Type,
        string Title,
        string Owner,
        string UpdatedBy,
        DateTimeOffset UpdatedAt,
        string? SeedStatus = null,
        decimal? SeedProgressPercent = null,
        DateOnly? TargetDate = null);

    private sealed record StrategicWorkItemSnapshot(
        StrategicWorkItemDefinition Definition,
        string Status,
        decimal ProgressPercent,
        string UpdatedBy,
        DateTimeOffset UpdatedAt)
    {
        public BoardWorkItemSummaryResponse ToSummary()
            => new(
                Definition.Id,
                Definition.ParentWorkItemId,
                Definition.Type,
                Definition.Title,
                Status,
                ProgressPercent,
                Definition.Owner,
                UpdatedBy,
                UpdatedAt);
    }
}
