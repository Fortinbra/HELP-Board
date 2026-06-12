namespace HelpBoard.Web.Services;

public sealed class DashboardFoundationService
{
    private static readonly DashboardSummaryCardModel[] FoundationCards =
    [
        new(
            Key: "foundation-overview",
            Title: "Foundation overview",
            ValueText: "Ready",
            SupportingText: "Baseline structure is in place for the dashboard surface.",
            IconName: "layout-dashboard",
            IconToneClass: "hb-stat-card__icon--primary",
            StatusText: "Foundational",
            StatusIconName: "sparkles",
            StatusBadgeClass: "hb-badge--info",
            DrillDownRoute: "tickets",
            DrillDownText: "Open workspace"),
        new(
            Key: "experience-signals",
            Title: "Experience signals",
            ValueText: "3 themes",
            SupportingText: "Placeholder guidance covers focus, flow, and information hierarchy.",
            IconName: "activity",
            IconToneClass: "hb-stat-card__icon--warning",
            StatusText: "In progress",
            StatusIconName: "clock-3",
            StatusBadgeClass: "hb-badge--warning",
            DrillDownRoute: "strategic-plan",
            DrillDownText: "Review themes"),
        new(
            Key: "navigation-readiness",
            Title: "Navigation readiness",
            ValueText: "Preview",
            SupportingText: "Routing and surface entry points are ready for future content.",
            IconName: "menu",
            IconToneClass: "hb-stat-card__icon--primary",
            StatusText: "Ready",
            StatusIconName: "check-circle-2",
            StatusBadgeClass: "hb-badge--success",
            DrillDownRoute: "boards",
            DrillDownText: "Inspect routes"),
        new(
            Key: "delivery-frames",
            Title: "Delivery frames",
            ValueText: "Foundation",
            SupportingText: "This slice is intentionally lightweight and source-agnostic.",
            IconName: "panel-left",
            IconToneClass: "hb-stat-card__icon--danger",
            StatusText: "Placeholder",
            StatusIconName: "inbox",
            StatusBadgeClass: "hb-badge--neutral",
            DrillDownRoute: "tickets/active",
            DrillDownText: "See surface")
    ];

    private static readonly DashboardWidgetModel[] FoundationWidgets =
    [
        new(
            Key: "work-intake-flow",
            Title: "Work intake flow",
            WidgetTypeLabel: "Pipeline widget",
            Description: "Placeholder intake metrics for triage volume and response timing across queues.",
            StateLabel: "Sample data",
            StateBadgeClass: "hb-badge--neutral",
            StateIconName: "inbox",
            DrillDownRoute: "tickets",
            DrillDownText: "Open ticket board",
            RefreshStrategyLabel: "Refresh strategy: static placeholder snapshot on page load",
            AccessibilitySummary: "Text summary: Intake queue is stable with moderate volume and no blocked escalation path.",
            Metrics:
            [
                new(
                    Label: "New requests",
                    ValueText: "28",
                    TrendLabel: "Up 3 from prior check",
                    TrendBadgeClass: "hb-badge--warning",
                    TrendIconName: "activity"),
                new(
                    Label: "First response SLA",
                    ValueText: "94%",
                    TrendLabel: "Within expected range",
                    TrendBadgeClass: "hb-badge--success",
                    TrendIconName: "check-circle-2"),
                new(
                    Label: "Blocked escalations",
                    ValueText: "0",
                    TrendLabel: "No blockers detected",
                    TrendBadgeClass: "hb-badge--success",
                    TrendIconName: "check-circle-2")
            ]),
        new(
            Key: "delivery-health",
            Title: "Delivery health lens",
            WidgetTypeLabel: "Health widget",
            Description: "Placeholder cross-team execution signals for status confidence and aging work.",
            StateLabel: "Sample data",
            StateBadgeClass: "hb-badge--neutral",
            StateIconName: "inbox",
            DrillDownRoute: "boards",
            DrillDownText: "Inspect board health",
            RefreshStrategyLabel: "Refresh strategy: placeholder values reset on manual reload",
            AccessibilitySummary: "Text summary: Delivery confidence is watch-level due to aging items even though commitments are mostly on track.",
            Metrics:
            [
                new(
                    Label: "On-track commitments",
                    ValueText: "11",
                    TrendLabel: "Steady week over week",
                    TrendBadgeClass: "hb-badge--info",
                    TrendIconName: "activity"),
                new(
                    Label: "Watch items",
                    ValueText: "4",
                    TrendLabel: "2 recently added",
                    TrendBadgeClass: "hb-badge--warning",
                    TrendIconName: "triangle-alert"),
                new(
                    Label: "Aging > 14 days",
                    ValueText: "3",
                    TrendLabel: "Needs follow-up",
                    TrendBadgeClass: "hb-badge--danger",
                    TrendIconName: "alert-circle")
            ]),
        new(
            Key: "release-readiness",
            Title: "Release readiness pulse",
            WidgetTypeLabel: "Release widget",
            Description: "Placeholder quality and deployment readiness cues for the next publish window.",
            StateLabel: "Sample data",
            StateBadgeClass: "hb-badge--neutral",
            StateIconName: "inbox",
            DrillDownRoute: "tickets/active",
            DrillDownText: "Open active work",
            RefreshStrategyLabel: "Refresh strategy: sample values rotate with each service update",
            AccessibilitySummary: "Text summary: Release readiness is cautious with quality checks mostly passing and one deployment dependency outstanding.",
            Metrics:
            [
                new(
                    Label: "Quality checks passing",
                    ValueText: "19/21",
                    TrendLabel: "Improved since last run",
                    TrendBadgeClass: "hb-badge--info",
                    TrendIconName: "activity"),
                new(
                    Label: "Open release blockers",
                    ValueText: "1",
                    TrendLabel: "Dependency pending",
                    TrendBadgeClass: "hb-badge--warning",
                    TrendIconName: "clock-3"),
                new(
                    Label: "Rollback plan",
                    ValueText: "Drafted",
                    TrendLabel: "Review required",
                    TrendBadgeClass: "hb-badge--accent",
                    TrendIconName: "panel-left")
            ])
    ];

    public Task<DashboardSummaryModel> GetFoundationSummaryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(
            new DashboardSummaryModel(
                Cards: FoundationCards,
                Widgets: FoundationWidgets,
                IsPlaceholderData: true,
                DataSourceLabel: "Dashboard foundation sample placeholders (source-agnostic, non-persistent)",
                LastUpdatedAt: DateTimeOffset.UtcNow));
}