using Microsoft.AspNetCore.Components;
using HelpBoard.Web.Services;

namespace HelpBoard.Web.Layout;

public partial class NavMenu : ComponentBase
{
    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    private bool collapseNavMenu = true;

    private bool IsStrategicPlanEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsStrategicPlanEnabled = await FeatureToggleService.IsStrategicPlanEnabledAsync();
    }

    private string NavMenuCssClass => collapseNavMenu ? "is-collapsed" : string.Empty;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private void CloseNavMenu()
    {
        collapseNavMenu = true;
    }
}
