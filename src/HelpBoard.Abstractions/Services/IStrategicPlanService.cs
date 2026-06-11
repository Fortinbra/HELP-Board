using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Services;

/// <summary>Business operations for the reserved Strategic Plan project board.</summary>
public interface IStrategicPlanService
{
    Task<ProjectBoardResponse> GetStrategicBoardAsync(CancellationToken cancellationToken = default);

    Task<StrategicOverviewResponse> GetStrategicOverviewAsync(CancellationToken cancellationToken = default);

    Task<BoardWorkItemDetailResponse?> GetStrategicWorkItemDetailAsync(Guid workItemId, CancellationToken cancellationToken = default);
}
