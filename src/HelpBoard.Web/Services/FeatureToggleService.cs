using Microsoft.FeatureManagement;

namespace HelpBoard.Web.Services;

public interface IFeatureToggleService
{
    Task<bool> IsStrategicPlanEnabledAsync();

    Task<bool> IsEnabledAsync(string featureKey);
}

public static class FeatureToggleKeys
{
    public const string StrategicPlan = "StrategicPlan";
}

public sealed class FeatureToggleService : IFeatureToggleService
{
    private static readonly IReadOnlySet<string> SupportedFeatureKeys =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FeatureToggleKeys.StrategicPlan,
        };

    private readonly IFeatureManager featureManager;

    public FeatureToggleService(IFeatureManager featureManager)
    {
        this.featureManager = featureManager;
    }

    public Task<bool> IsStrategicPlanEnabledAsync()
    {
        return IsEnabledAsync(FeatureToggleKeys.StrategicPlan);
    }

    public async Task<bool> IsEnabledAsync(string featureKey)
    {
        if (string.IsNullOrWhiteSpace(featureKey))
        {
            return false;
        }

        if (!SupportedFeatureKeys.Contains(featureKey))
        {
            return false;
        }

        return await featureManager.IsEnabledAsync(featureKey);
    }
}