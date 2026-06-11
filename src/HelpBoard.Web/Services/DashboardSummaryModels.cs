namespace HelpBoard.Web.Services;

public sealed record DashboardSummaryModel(
    IReadOnlyList<DashboardSummaryCardModel> Cards,
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
