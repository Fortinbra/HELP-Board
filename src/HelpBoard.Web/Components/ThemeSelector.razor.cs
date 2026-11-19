using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class ThemeSelector
{
    private bool _isLoaded;
    private bool _isLightEnabled;
    private bool _isDarkEnabled;
    private bool _isHelpEnabled;
    private bool _isGeocitiesEnabled;
    private bool _isMinimalistEnabled;
    private bool _isWesternEnabled;

    [Inject]
    private IFeatureToggleService FeatureToggleService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        _isLightEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.LightTheme);
        _isDarkEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.DarkTheme);
        _isHelpEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.HelpTheme);
        _isGeocitiesEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.GeocitiesTheme);
        _isMinimalistEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.MinimalistTheme);
        _isWesternEnabled = await FeatureToggleService.IsEnabledAsync(FeatureToggleKeys.WesternTheme);
        _isLoaded = true;
    }
}
