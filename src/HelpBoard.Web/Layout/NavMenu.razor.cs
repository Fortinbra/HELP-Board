using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Layout;

public partial class NavMenu : ComponentBase
{
    private bool collapseNavMenu = true;

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
