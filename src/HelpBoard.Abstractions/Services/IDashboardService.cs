using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Services;

/// <summary>Provides aggregate dashboard data for read-only UI experiences.</summary>
public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
}