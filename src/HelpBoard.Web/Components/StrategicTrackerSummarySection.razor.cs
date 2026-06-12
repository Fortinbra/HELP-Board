using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class StrategicTrackerSummarySection : ComponentBase
{
    [Parameter, EditorRequired]
    public IReadOnlyList<DashboardSummaryCardModel> Cards { get; set; } = [];

    [Parameter, EditorRequired]
    public string LastUpdatedText { get; set; } = string.Empty;

    [Parameter]
    public bool IsPlaceholderData { get; set; }

    [Parameter]
    public string DataSourceLabel { get; set; } = string.Empty;
}
