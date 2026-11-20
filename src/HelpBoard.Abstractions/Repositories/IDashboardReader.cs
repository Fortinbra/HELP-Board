using HelpBoard.Abstractions.Domain;

namespace HelpBoard.Abstractions.Repositories;

/// <summary>Reads shared dashboards.</summary>
public interface IDashboardReader
{
    Task<IReadOnlyList<DashboardDocument>> GetAllAsync(CancellationToken cancellationToken);
    Task<DashboardDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
