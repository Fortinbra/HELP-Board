using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Layout;

public partial class MainLayout
{
    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    private bool IsHelloWorldBaselineEnabled { get; set; }
    private bool IsLightEnabled { get; set; }
    private bool IsDarkEnabled { get; set; }
    private bool IsHelpEnabled { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsHelloWorldBaselineEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.HelloWorldBaseline);
        if (!IsHelloWorldBaselineEnabled)
        {
            return;
        }

        IsLightEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.LightTheme);
        IsDarkEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.DarkTheme);
        IsHelpEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.HelpTheme);
    }
}
