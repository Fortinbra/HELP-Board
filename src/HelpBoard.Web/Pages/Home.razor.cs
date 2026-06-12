using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private DashboardFoundationService DashboardFoundation { get; set; } = null!;

    [Inject]
    private StrategicPlanApiService StrategicPlanApi { get; set; } = null!;

    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    private string? ErrorMessage { get; set; }

    private DateTimeOffset? LastUpdatedAt { get; set; }

    private DateTimeOffset? StrategicLastUpdatedAt { get; set; }

    private IReadOnlyList<DashboardSummaryCardModel> SummaryCards { get; set; } = [];

    private IReadOnlyList<DashboardSummaryCardModel> StrategicSummaryCards { get; set; } = [];

    private bool IsStrategicPlaceholderData { get; set; }

    private string StrategicDataSourceLabel { get; set; } = string.Empty;

    private bool IsStrategicPlanEnabled { get; set; }

    private bool ShowStrategicSection => IsStrategicPlanEnabled && StrategicSummaryCards.Count > 0;

    private string LastUpdatedText => LastUpdatedAt is null
        ? "Last updated: --"
        : $"Last updated: {LastUpdatedAt.Value.LocalDateTime:yyyy-MM-dd HH:mm}";

    private string StrategicLastUpdatedText => StrategicLastUpdatedAt is null
        ? "Last updated: --"
        : $"Last updated: {StrategicLastUpdatedAt.Value.LocalDateTime:yyyy-MM-dd HH:mm}";

    protected override async Task OnInitializedAsync()
    {
        IsStrategicPlanEnabled = FeatureToggleService.IsStrategicPlanEnabled();
        await LoadSummaryAsync();
    }

    private async Task ReloadSummaryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        await LoadSummaryAsync();
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var summary = await DashboardFoundation.GetFoundationSummaryAsync();

            SummaryCards = summary.Cards;
            LastUpdatedAt = summary.LastUpdatedAt;

            if (!IsStrategicPlanEnabled)
            {
                StrategicSummaryCards = [];
                StrategicLastUpdatedAt = null;
                IsStrategicPlaceholderData = false;
                StrategicDataSourceLabel = string.Empty;

                return;
            }

            var strategicSummary = await StrategicPlanApi.GetDashboardSummaryAsync();

            StrategicSummaryCards =
            [
                strategicSummary.StrategicHealthCard,
                strategicSummary.ObjectivesAtRiskCard,
                strategicSummary.MilestonesDueSoonCard
            ];
            StrategicLastUpdatedAt = strategicSummary.LastUpdatedAt;
            IsStrategicPlaceholderData = strategicSummary.IsPlaceholderData;
            StrategicDataSourceLabel = strategicSummary.DataSourceLabel;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard foundation content: {ex.Message}";
            SummaryCards = [];
            LastUpdatedAt = null;
            StrategicSummaryCards = [];
            StrategicLastUpdatedAt = null;
            IsStrategicPlaceholderData = false;
            StrategicDataSourceLabel = string.Empty;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
