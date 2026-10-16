using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Layout;

public partial class BaselineShell
{
    private ElementReference _mainContent;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool IsLightEnabled { get; set; } = true;

    [Parameter]
    public bool IsDarkEnabled { get; set; } = true;

    [Parameter]
    public bool IsHelpEnabled { get; set; } = true;

    private async Task FocusMainAsync() => await _mainContent.FocusAsync();
}
