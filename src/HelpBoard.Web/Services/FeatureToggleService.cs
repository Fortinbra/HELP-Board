using Microsoft.Extensions.Configuration;

namespace HelpBoard.Web.Services;

public interface IFeatureToggleService
{
    bool IsStrategicPlanEnabled();
}

public sealed class FeatureToggleService : IFeatureToggleService
{
    private const string StrategicPlanToggleKey = "FeatureToggles:StrategicPlan:Enabled";

    private readonly IConfiguration configuration;

    public FeatureToggleService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public bool IsStrategicPlanEnabled()
    {
        var configuredValue = configuration[StrategicPlanToggleKey];

        return bool.TryParse(configuredValue, out var isEnabled) && isEnabled;
    }
}