using System.Net.Http.Json;
using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

public sealed class StrategicPlanApiService(HttpClient httpClient)
{
    private const string BoardRoute = "api/strategic/board";
    private static readonly DateTimeOffset PlaceholderSnapshot = new(2026, 6, 11, 14, 30, 0, TimeSpan.Zero);

    public async Task<StrategicOverviewModel> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var board = await TryGetJsonAsync<ProjectBoardResponse>(BoardRoute, cancellationToken).ConfigureAwait(false);

        return board is null
            ? CreatePlaceholderOverview()
            : CreateOverview(board);
    }

    public async Task<StrategicDashboardSummaryModel> GetDashboardSummaryAsync(CancellationToken cancellationToken = default)
    {
        var overview = await GetOverviewAsync(cancellationToken).ConfigureAwait(false);

        var objectiveCount = overview.Objectives.Count;
        var atRiskCount = overview.Objectives.Count(item => item.IsAtRisk);
        var averageCompletion = objectiveCount == 0
            ? 0
            : (int)Math.Round(overview.Objectives.Average(item => item.CompletionPercent));
        var milestonePressureCount = overview.UpcomingMilestones.Count(item => IsAttentionStatus(item.StatusText) || item.CompletionPercent < 100);

        var summary = new StrategicDashboardSummaryModel(
            StrategicHealthCard: new DashboardSummaryCardModel(
                Key: "strategic-health",
                Title: "Strategic health summary",
                ValueText: $"{averageCompletion}%",
                SupportingText: objectiveCount == 0
                    ? "No strategic objectives are available yet."
                    : $"Average completion across {objectiveCount} objectives.",
                IconName: "target",
                IconToneClass: "hb-stat-card__icon--primary",
                StatusText: atRiskCount == 0 ? "Stable" : "Watch",
                StatusIconName: atRiskCount == 0 ? "check-circle-2" : "triangle-alert",
                StatusBadgeClass: atRiskCount == 0 ? "hb-badge--success" : "hb-badge--warning",
                DrillDownRoute: "strategic-plan",
                DrillDownText: "Open strategic board"),
            ObjectivesAtRiskCard: new DashboardSummaryCardModel(
                Key: "objectives-at-risk",
                Title: "Objectives at risk",
                ValueText: atRiskCount.ToString(),
                SupportingText: atRiskCount == 0
                    ? "No objectives are currently at risk."
                    : "Objective work items that need leadership attention this week.",
                IconName: "triangle-alert",
                IconToneClass: atRiskCount == 0 ? "hb-stat-card__icon--success" : "hb-stat-card__icon--danger",
                StatusText: atRiskCount == 0 ? "On track" : "Needs action",
                StatusIconName: atRiskCount == 0 ? "check-circle-2" : "alert-circle",
                StatusBadgeClass: atRiskCount == 0 ? "hb-badge--success" : "hb-badge--danger",
                DrillDownRoute: "strategic-plan#objective-board",
                DrillDownText: atRiskCount == 0 ? "Review strategic board" : "Locate at-risk objectives"),
            MilestonesDueSoonCard: new DashboardSummaryCardModel(
                Key: "milestones-due-soon",
                Title: "Milestones needing attention",
                ValueText: milestonePressureCount.ToString(),
                SupportingText: "Incomplete or at-risk milestone work items on the strategic board.",
                IconName: "calendar-clock",
                IconToneClass: milestonePressureCount == 0 ? "hb-stat-card__icon--success" : "hb-stat-card__icon--warning",
                StatusText: milestonePressureCount == 0 ? "Clear" : "Active",
                StatusIconName: milestonePressureCount == 0 ? "check-circle-2" : "triangle-alert",
                StatusBadgeClass: milestonePressureCount == 0 ? "hb-badge--success" : "hb-badge--warning",
                DrillDownRoute: "strategic-plan#milestone-board",
                DrillDownText: "Open milestone board"),
            LastUpdatedAt: overview.LastUpdatedAt,
            IsPlaceholderData: overview.IsPlaceholderData,
            DataSourceLabel: overview.DataSourceLabel);

        return summary;
    }

    private static StrategicOverviewModel CreateOverview(ProjectBoardResponse board)
    {
        var items = board.WorkItems ?? [];
        var itemsById = items.ToDictionary(item => item.Id);
        var childrenByParent = items
            .Where(item => item.ParentWorkItemId is not null)
            .GroupBy(item => item.ParentWorkItemId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());

        var objectives = items
            .Where(item => IsType(item.Type, "objective"))
            .OrderBy(item => item.Title)
            .Select(item => CreateObjectiveSummary(item, itemsById, childrenByParent))
            .ToList();

        var milestones = items
            .Where(item => IsType(item.Type, "milestone"))
            .OrderBy(item => item.Title)
            .Select(item => CreateMilestoneSummary(item, itemsById))
            .ToList();

        var ownership = objectives
            .GroupBy(item => item.OwnerName)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var latestUpdate = group.OrderByDescending(item => item.UpdatedAt).First();

                return new OwnershipAllocationModel(
                    Name: group.Key,
                    Role: "Strategic owner",
                    ObjectiveCount: group.Count(),
                    AtRiskCount: group.Count(item => item.IsAtRisk),
                    ContactLabel: $"Updated by {latestUpdate.UpdatedBy} on {latestUpdate.UpdatedAt.LocalDateTime:yyyy-MM-dd}");
            })
            .ToList();

        return new StrategicOverviewModel(
            Board: board,
            Objectives: objectives,
            UpcomingMilestones: milestones,
            OwnershipAllocation: ownership,
            LastUpdatedAt: board.UpdatedAt,
            IsPlaceholderData: false,
            DataSourceLabel: $"Live strategic board data: {board.Name}");
    }

    private static StrategicObjectiveSummaryModel CreateObjectiveSummary(
        BoardWorkItemSummaryResponse objective,
        IReadOnlyDictionary<Guid, BoardWorkItemSummaryResponse> itemsById,
        IReadOnlyDictionary<Guid, List<BoardWorkItemSummaryResponse>> childrenByParent)
    {
        var initiativeChildren = GetChildren(objective.Id, childrenByParent, "initiative");
        var openMilestones = CountOpenMilestones(objective.Id, childrenByParent);
        var completionPercent = GetRolledUpProgress(objective.Id, itemsById, childrenByParent);

        return new StrategicObjectiveSummaryModel(
            WorkItemId: objective.Id,
            Title: objective.Title,
            OwnerName: objective.Owner,
            StatusText: ToStatusText(objective.Status),
            StatusBadgeClass: GetStatusBadgeClass(objective.Status),
            StatusIconName: GetStatusIconName(objective.Status),
            CompletionPercent: completionPercent,
            InitiativeCount: initiativeChildren.Count,
            OpenMilestoneCount: openMilestones,
            HealthSummaryText: $"Latest update by {objective.UpdatedBy} on {objective.UpdatedAt.LocalDateTime:yyyy-MM-dd}.",
            UpdatedBy: objective.UpdatedBy,
            UpdatedAt: objective.UpdatedAt,
            IsAtRisk: IsAtRiskStatus(objective.Status),
            DrillDownRoute: $"strategic-plan/{objective.Id:D}");
    }

    private static MilestoneTimelineItemModel CreateMilestoneSummary(
        BoardWorkItemSummaryResponse milestone,
        IReadOnlyDictionary<Guid, BoardWorkItemSummaryResponse> itemsById)
    {
        var objective = FindAncestorObjective(milestone, itemsById);
        var objectiveId = objective?.Id ?? milestone.Id;

        return new MilestoneTimelineItemModel(
            WorkItemId: milestone.Id,
            ObjectiveWorkItemId: objectiveId,
            ObjectiveTitle: objective?.Title ?? milestone.Title,
            Label: milestone.Title,
            StatusText: ToStatusText(milestone.Status),
            StatusBadgeClass: GetStatusBadgeClass(milestone.Status),
            StatusIconName: GetStatusIconName(milestone.Status),
            CheckpointSummary: $"Latest update by {milestone.UpdatedBy} on {milestone.UpdatedAt.LocalDateTime:yyyy-MM-dd}.",
            CompletionPercent: (int)Math.Round(milestone.ProgressPercent),
            UpdatedBy: milestone.UpdatedBy,
            UpdatedAt: milestone.UpdatedAt,
            DetailRoute: objective is null
                ? $"strategic-plan/{milestone.Id:D}"
                : $"strategic-plan/{objective.Id:D}#objective-{objective.Id:D}");
    }

    private static int GetRolledUpProgress(
        Guid itemId,
        IReadOnlyDictionary<Guid, BoardWorkItemSummaryResponse> itemsById,
        IReadOnlyDictionary<Guid, List<BoardWorkItemSummaryResponse>> childrenByParent)
    {
        if (!childrenByParent.TryGetValue(itemId, out var children) || children.Count == 0)
        {
            return (int)Math.Round(itemsById[itemId].ProgressPercent);
        }

        var childProgress = children.Select(child => GetRolledUpProgress(child.Id, itemsById, childrenByParent));
        return (int)Math.Round(childProgress.Average());
    }

    private static int CountOpenMilestones(
        Guid itemId,
        IReadOnlyDictionary<Guid, List<BoardWorkItemSummaryResponse>> childrenByParent)
    {
        if (!childrenByParent.TryGetValue(itemId, out var children) || children.Count == 0)
        {
            return 0;
        }

        return children.Sum(child => IsType(child.Type, "milestone")
            ? IsCompleteStatus(child.Status) ? 0 : 1
            : CountOpenMilestones(child.Id, childrenByParent));
    }

    private static IReadOnlyList<BoardWorkItemSummaryResponse> GetChildren(
        Guid itemId,
        IReadOnlyDictionary<Guid, List<BoardWorkItemSummaryResponse>> childrenByParent,
        string type)
        => childrenByParent.TryGetValue(itemId, out var children)
            ? children.Where(child => IsType(child.Type, type)).ToList()
            : [];

    private static BoardWorkItemSummaryResponse? FindAncestorObjective(
        BoardWorkItemSummaryResponse workItem,
        IReadOnlyDictionary<Guid, BoardWorkItemSummaryResponse> itemsById)
    {
        var current = workItem;

        while (current.ParentWorkItemId is not null)
        {
            if (!itemsById.TryGetValue(current.ParentWorkItemId.Value, out var parent))
            {
                return null;
            }

            if (IsType(parent.Type, "objective"))
            {
                return parent;
            }

            current = parent;
        }

        return null;
    }

    private static bool IsAttentionStatus(string status)
        => IsAtRiskStatus(status) || IsBlockedStatus(status) || !IsCompleteStatus(status);

    private static bool IsAtRiskStatus(string status)
        => IsType(status, "AtRisk");

    private static bool IsBlockedStatus(string status)
        => IsType(status, "Blocked");

    private static bool IsCompleteStatus(string status)
        => IsType(status, "Completed");

    private static bool IsType(string value, string expected)
        => string.Equals(value, expected, StringComparison.OrdinalIgnoreCase);

    private static string ToStatusText(string status)
        => status switch
        {
            var value when IsType(value, "OnTrack") => "On track",
            var value when IsType(value, "AtRisk") => "At risk",
            var value when IsType(value, "Blocked") => "Blocked",
            var value when IsType(value, "Completed") => "Completed",
            var value when IsType(value, "Planned") => "Planned",
            _ => status
        };

    private static string GetStatusBadgeClass(string status)
        => status switch
        {
            var value when IsType(value, "OnTrack") => "hb-badge--success",
            var value when IsType(value, "Completed") => "hb-badge--success",
            var value when IsType(value, "AtRisk") => "hb-badge--warning",
            var value when IsType(value, "Blocked") => "hb-badge--danger",
            var value when IsType(value, "Planned") => "hb-badge--neutral",
            _ => "hb-badge--info"
        };

    private static string GetStatusIconName(string status)
        => status switch
        {
            var value when IsType(value, "OnTrack") => "check-circle-2",
            var value when IsType(value, "Completed") => "check-circle-2",
            var value when IsType(value, "AtRisk") => "triangle-alert",
            var value when IsType(value, "Blocked") => "alert-circle",
            var value when IsType(value, "Planned") => "clock-3",
            _ => "activity"
        };

    private async Task<T?> TryGetJsonAsync<T>(string route, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetFromJsonAsync<T>(route, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException)
        {
            return default;
        }
        catch (NotSupportedException)
        {
            return default;
        }
    }

    private static StrategicOverviewModel CreatePlaceholderOverview()
    {
        var board = CreatePlaceholderBoard();

        var overview = CreateOverview(board);

        return overview with
        {
            LastUpdatedAt = PlaceholderSnapshot,
            IsPlaceholderData = true,
            DataSourceLabel = "Fallback strategic board data"
        };
    }

    private static ProjectBoardResponse CreatePlaceholderBoard()
        => new(
            Guid.Parse("00000000-0000-0000-0000-000000000003"),
            "strategic-plan",
            "Strategic Plan",
            IsSystemBoard: true,
            IsDeletable: false,
            PlaceholderSnapshot,
            [
                new BoardWorkItemSummaryResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), null, "theme", "Platform Excellence", "OnTrack", 60m, "CTO", "cto", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("22222222-2222-2222-2222-222222222222"), null, "theme", "Delivery Acceleration", "AtRisk", 40m, "CTO", "cto", PlaceholderSnapshot),

                new BoardWorkItemSummaryResponse(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Guid.Parse("11111111-1111-1111-1111-111111111111"), "objective", "Increase service reliability", "OnTrack", 50m, "Platform Director", "platform.director", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Guid.Parse("22222222-2222-2222-2222-222222222222"), "objective", "Reduce engineering lead time", "AtRisk", 30m, "Engineering Director", "engineering.director", PlaceholderSnapshot),

                new BoardWorkItemSummaryResponse(Guid.Parse("10000000-0000-0000-0000-000000000001"), Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "initiative", "Improve observability coverage", "AtRisk", 50m, "Platform Team", "platform.lead", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("10000000-0000-0000-0000-000000000002"), Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "initiative", "Reduce mean time to recovery", "OnTrack", 80m, "SRE Team", "sre.manager", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("20000000-0000-0000-0000-000000000001"), Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "initiative", "Shorten CI cycle time", "Blocked", 10m, "Developer Experience", "devex.lead", PlaceholderSnapshot),

                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000001"), Guid.Parse("10000000-0000-0000-0000-000000000001"), "milestone", "Distributed tracing coverage", "OnTrack", 60m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000002"), Guid.Parse("10000000-0000-0000-0000-000000000001"), "milestone", "SLO dashboard published", "AtRisk", 40m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000003"), Guid.Parse("10000000-0000-0000-0000-000000000002"), "milestone", "Runbook training completed", "Completed", 100m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000004"), Guid.Parse("10000000-0000-0000-0000-000000000002"), "milestone", "Escalation policy finalized", "OnTrack", 60m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000005"), Guid.Parse("20000000-0000-0000-0000-000000000001"), "milestone", "Build cache optimization", "Blocked", 20m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot),
                new BoardWorkItemSummaryResponse(Guid.Parse("30000000-0000-0000-0000-000000000006"), Guid.Parse("20000000-0000-0000-0000-000000000001"), "milestone", "Parallel test execution", "Planned", 0m, "Workstream Owner", "strategy.pm", PlaceholderSnapshot)
            ]);
}
