using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class DashboardSummarySection : ComponentBase
{
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string Description { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public string LastUpdatedText { get; set; } = string.Empty;

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? ErrorMessage { get; set; }

    [Parameter]
    public IReadOnlyList<DashboardSummaryCardModel> Cards { get; set; } = [];

    [Parameter]
    public EventCallback OnRetry { get; set; }

    private async Task OnRetryClickAsync()
    {
        if (OnRetry.HasDelegate)
        {
            await OnRetry.InvokeAsync();
        }
    }
}
