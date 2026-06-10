using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Pages;

public partial class Home : ComponentBase
{
    [Inject]
    private TicketApiService TicketApi { get; set; } = null!;

    private bool IsLoading { get; set; } = true;

    private string? ErrorMessage { get; set; }

    private DateTimeOffset? LastUpdatedAt { get; set; }

    private IReadOnlyList<DashboardSummaryCardModel> SummaryCards { get; set; } = [];

    private string LastUpdatedText => LastUpdatedAt is null
        ? "Last updated: --"
        : $"Last updated: {LastUpdatedAt.Value.LocalDateTime:yyyy-MM-dd HH:mm}";

    protected override async Task OnInitializedAsync()
    {
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
            var summary = await TicketApi.GetDashboardSummaryAsync();
            SummaryCards = summary.Cards;
            LastUpdatedAt = summary.LastUpdatedAt;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard summary: {ex.Message}";
            SummaryCards = [];
            LastUpdatedAt = null;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
