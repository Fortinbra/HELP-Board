using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class DashboardWidgetSection : ComponentBase
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Description { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string LastUpdatedText { get; set; } = string.Empty;

    [Parameter]
    public bool IsPlaceholderData { get; set; }

    [Parameter]
    public string DataSourceLabel { get; set; } = string.Empty;

    [Parameter]
    public IReadOnlyList<DashboardWidgetModel> Widgets { get; set; } = [];
}
