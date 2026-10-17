using System.Net.Http;
using System.Text.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Pages;

public partial class Dashboard : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private AuthenticationStatusResponse? _status;
    private string? _errorMessage;
    private string? DashboardErrorMessage { get; set; }
    private bool _isCheckingAccess = true;
    private bool IsLoading { get; set; }
    private bool IsHelloWorldBaselineEnabled { get; set; }
    private bool IsStrategicPlanEnabled { get; set; }
    private DateTimeOffset? LastUpdatedAt { get; set; }
    private DateTimeOffset? StrategicLastUpdatedAt { get; set; }
    private IReadOnlyList<DashboardSummaryCardModel> SummaryCards { get; set; } = [];
    private IReadOnlyList<DashboardSummaryCardModel> StrategicSummaryCards { get; set; } = [];
    private bool IsStrategicPlaceholderData { get; set; }
    private string StrategicDataSourceLabel { get; set; } = string.Empty;
    private bool ShowStrategicSection => IsStrategicPlanEnabled && StrategicSummaryCards.Count > 0;

    private string LastUpdatedText => LastUpdatedAt is null
        ? "Last updated: --"
        : $"Last updated: {LastUpdatedAt.Value.LocalDateTime:yyyy-MM-dd HH:mm}";

    private string StrategicLastUpdatedText => StrategicLastUpdatedAt is null
        ? "Last updated: --"
        : $"Last updated: {StrategicLastUpdatedAt.Value.LocalDateTime:yyyy-MM-dd HH:mm}";

    [Inject]
    private IAuthenticationClient AuthenticationClient { get; set; } = null!;

    [Inject]
    private DashboardFoundationService DashboardFoundation { get; set; } = null!;

    [Inject]
    private StrategicPlanApiService StrategicPlanApi { get; set; } = null!;

    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    [Inject]
    private ILogger<Dashboard> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _status = await AuthenticationClient.GetStatusAsync(_cancellationTokenSource.Token);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Unable to verify dashboard access.");
            _errorMessage = "Dashboard access could not be verified. Please try again later.";
            return;
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The authentication status response was invalid.");
            _errorMessage = "Dashboard access could not be verified. Please try again later.";
            return;
        }
        finally
        {
            _isCheckingAccess = false;
        }

        if (_status is not { IsAuthenticated: true })
        {
            return;
        }

        IsHelloWorldBaselineEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.HelloWorldBaseline);
        if (IsHelloWorldBaselineEnabled)
        {
            return;
        }

        IsLoading = true;
        IsStrategicPlanEnabled = await FeatureToggleService.IsStrategicPlanEnabledAsync();
        await LoadSummaryAsync();
    }

    private async Task ReloadSummaryAsync()
    {
        IsLoading = true;
        DashboardErrorMessage = null;
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
                ClearStrategicSummary();
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
        catch (Exception exception)
        {
            Logger.LogError(exception, "Failed to load dashboard foundation content.");
            DashboardErrorMessage = "Dashboard content could not be loaded. Please try again.";
            SummaryCards = [];
            LastUpdatedAt = null;
            ClearStrategicSummary();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ClearStrategicSummary()
    {
        StrategicSummaryCards = [];
        StrategicLastUpdatedAt = null;
        IsStrategicPlaceholderData = false;
        StrategicDataSourceLabel = string.Empty;
    }

    private void StartSignIn()
    {
        NavigationManager.NavigateTo("/auth/sign-in?returnUrl=%2Fdashboard", forceLoad: true);
    }

    private void ReturnToLanding()
    {
        NavigationManager.NavigateTo("/", forceLoad: true);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
