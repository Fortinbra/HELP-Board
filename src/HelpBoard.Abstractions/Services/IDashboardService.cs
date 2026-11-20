using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Services;

/// <summary>Manages shared dashboard configurations.</summary>
public interface IDashboardService
{
    Task<IReadOnlyList<DashboardResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<DashboardResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DashboardResponse> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken);
    Task<DashboardUpdateResult> UpdateAsync(Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken);
}
