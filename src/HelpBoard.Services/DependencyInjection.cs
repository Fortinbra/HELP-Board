using HelpBoard.Abstractions.Services;
using HelpBoard.Services.Strategic;
using HelpBoard.Services.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace HelpBoard.Services;

/// <summary>Extension methods for registering service implementations.</summary>
public static class DependencyInjection
{
    /// <summary>Registers all application services with the DI container.</summary>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITicketService, TicketService>();
        services.AddSingleton<IStrategicPlanService, StrategicPlanService>();

        return services;
    }
}
