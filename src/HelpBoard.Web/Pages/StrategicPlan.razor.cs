using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Pages;

public partial class StrategicPlan : ComponentBase
{
    [Inject]
    private StrategicPlanApiService StrategicPlanApi { get; set; } = null!;

    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    [Parameter]
    public Guid? WorkItemId { get; set; }

    [Parameter]
    public string? ObjectiveId { get; set; }

    private StrategicOverviewModel? Overview { get; set; }

    private bool IsLoading { get; set; } = true;

    private string? ErrorMessage { get; set; }

    private string? ObjectiveRouteMessage { get; set; }

    private bool IsStrategicPlanEnabled { get; set; }

    private int AverageCompletionPercent => Overview is null || Overview.Objectives.Count == 0
        ? 0
        : (int)Math.Round(Overview.Objectives.Average(item => item.CompletionPercent));

    private int AtRiskObjectiveCount => Overview?.Objectives.Count(item => item.IsAtRisk) ?? 0;

    private int MilestoneAttentionCount => Overview?.UpcomingMilestones.Count(item => item.CompletionPercent < 100 || !string.Equals(item.StatusText, "Completed", StringComparison.OrdinalIgnoreCase)) ?? 0;

    private string AverageCompletionStyle => $"width: {AverageCompletionPercent}%;";

    private string MilestoneAttentionStyle => Overview is null || Overview.UpcomingMilestones.Count == 0
        ? "width: 0%;"
        : $"width: {(int)Math.Round((double)MilestoneAttentionCount * 100 / Overview.UpcomingMilestones.Count)}%;";

    protected override async Task OnParametersSetAsync()
    {
        IsStrategicPlanEnabled = FeatureToggleService.IsStrategicPlanEnabled();

        if (!IsStrategicPlanEnabled)
        {
            IsLoading = false;
            ErrorMessage = null;
            Overview = null;
            ObjectiveRouteMessage = null;

            return;
        }

        await LoadOverviewAsync();
    }

    private async Task ReloadOverviewAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        await LoadOverviewAsync();
    }

    private async Task LoadOverviewAsync()
    {
        try
        {
            Overview = await StrategicPlanApi.GetOverviewAsync();
            ObjectiveRouteMessage = ResolveObjectiveRouteMessage();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Unable to load strategic overview board: {ex.Message}";
            Overview = null;
            ObjectiveRouteMessage = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string? ResolveObjectiveRouteMessage()
    {
        var selectedWorkItemId = GetSelectedWorkItemId();

        if (Overview is null || selectedWorkItemId is null)
        {
            return null;
        }

        var target = Overview.Objectives
            .FirstOrDefault(item => item.WorkItemId == selectedWorkItemId.Value);

        return target is null
            ? $"Work item '{selectedWorkItemId}' is not present in the current strategic board data set."
            : $"Showing objective '{target.Title}' within the strategic board.";
    }

    private bool IsFocusedObjective(StrategicObjectiveSummaryModel objective)
        => GetSelectedWorkItemId() is Guid selectedWorkItemId
           && objective.WorkItemId == selectedWorkItemId;

    private Guid? GetSelectedWorkItemId()
        => WorkItemId ?? (Guid.TryParse(ObjectiveId, out var legacyObjectiveId) ? legacyObjectiveId : null);
}
