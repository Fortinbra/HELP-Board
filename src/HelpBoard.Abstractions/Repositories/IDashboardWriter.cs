using HelpBoard.Abstractions.Domain;

namespace HelpBoard.Abstractions.Repositories;

/// <summary>Persists shared dashboards with optimistic concurrency.</summary>
public interface IDashboardWriter
{
    Task AddAsync(DashboardDocument dashboard, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(DashboardDocument dashboard, Guid expectedRevision, CancellationToken cancellationToken);
}
