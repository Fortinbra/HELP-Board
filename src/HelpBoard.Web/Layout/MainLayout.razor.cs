using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Layout;

public partial class MainLayout
{
    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    private bool IsHelloWorldBaselineEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsHelloWorldBaselineEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.HelloWorldBaseline);
    }
}
