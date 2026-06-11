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

    public Task<DashboardSummaryModel> GetFoundationSummaryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new DashboardSummaryModel(FoundationCards, DateTimeOffset.UtcNow));
}