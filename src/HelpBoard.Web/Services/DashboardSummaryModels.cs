namespace HelpBoard.Web.Services;

public sealed record DashboardSummaryModel(
    IReadOnlyList<DashboardSummaryCardModel> Cards,
    IReadOnlyList<DashboardWidgetModel> Widgets,
    bool IsPlaceholderData,
    string DataSourceLabel,
    DateTimeOffset LastUpdatedAt);

public sealed record DashboardSummaryCardModel(
    string Key,
    string Title,
    string ValueText,
    string SupportingText,
    string IconName,
    string IconToneClass,
    string StatusText,
    string StatusIconName,
    string StatusBadgeClass,
    string DrillDownRoute,
    string DrillDownText);

public sealed record DashboardWidgetModel(
    string Key,
    string Title,
    string WidgetTypeLabel,
    string Description,
    string StateLabel,
    string StateBadgeClass,
    string StateIconName,
    string DrillDownRoute,
    string DrillDownText,
    string RefreshStrategyLabel,
    string AccessibilitySummary,
    IReadOnlyList<DashboardWidgetMetricModel> Metrics);

public sealed record DashboardWidgetMetricModel(
    string Label,
    string ValueText,
    string TrendLabel,
    string TrendBadgeClass,
    string TrendIconName);
