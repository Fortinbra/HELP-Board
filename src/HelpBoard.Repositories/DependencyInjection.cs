using HelpBoard.Abstractions.Repositories;
using HelpBoard.Repositories.Data;
using HelpBoard.Repositories.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpBoard.Repositories;

/// <summary>Extension methods for registering repository services.</summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers the database context and all repository implementations with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public static IServiceCollection AddRepositories(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITicketReader, TicketRepository>();
        services.AddScoped<ITicketWriter, TicketRepository>();

        return services;
    }
}
