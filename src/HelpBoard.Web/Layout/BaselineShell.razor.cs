using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Layout;

public partial class BaselineShell
{
    private ElementReference _mainContent;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    private async Task FocusMainAsync() => await _mainContent.FocusAsync();
}
