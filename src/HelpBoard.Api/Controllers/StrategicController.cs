using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HelpBoard.Api.Controllers;

/// <summary>Provides endpoints for the reserved Strategic Plan board.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class StrategicController(IStrategicPlanService strategicPlanService) : ControllerBase
{
    /// <summary>Gets the reserved Strategic Plan board and its work items.</summary>
    [HttpGet("board")]
    [ProducesResponseType<ProjectBoardResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoardAsync(CancellationToken cancellationToken)
    {
        var board = await strategicPlanService.GetStrategicBoardAsync(cancellationToken);
        return Ok(board);
    }

    /// <summary>Gets the strategic overview feed used by the dashboard and strategic plan page.</summary>
    [HttpGet("overview")]
    [ProducesResponseType<StrategicOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var overview = await strategicPlanService.GetStrategicOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    /// <summary>Gets a strategic work item detail and its direct child work items.</summary>
    [HttpGet("work-items/{workItemId:guid}")]
    [ProducesResponseType<BoardWorkItemDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkItemAsync(Guid workItemId, CancellationToken cancellationToken)
    {
        var workItem = await strategicPlanService.GetStrategicWorkItemDetailAsync(workItemId, cancellationToken);

        if (workItem is null)
        {
            return NotFound();
        }

        return Ok(workItem);
    }
}
