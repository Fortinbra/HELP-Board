using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HelpBoard.Api.Controllers;

/// <summary>Provides aggregate dashboard data for dashboard widgets.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    /// <summary>Gets dashboard summary KPI and ticket status distribution.</summary>
    [HttpGet("summary")]
    [ProducesResponseType<DashboardSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var summary = await dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}