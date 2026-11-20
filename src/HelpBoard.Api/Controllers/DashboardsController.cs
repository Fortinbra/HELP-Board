using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpBoard.Api.Controllers;

/// <summary>Manages shared dashboards for authenticated users.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class DashboardsController(IDashboardService dashboardService, IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Lists shared dashboards.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DashboardResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
        => Ok(await dashboardService.GetAllAsync(cancellationToken));

    /// <summary>Gets a shared dashboard.</summary>
    [HttpGet("{id:guid}", Name = "GetDashboardById")]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var dashboard = await dashboardService.GetByIdAsync(id, cancellationToken);
        return dashboard is null ? NotFound() : Ok(dashboard);
    }

    /// <summary>Creates a shared dashboard.</summary>
    [HttpPost]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CreateDashboardRequest request, CancellationToken cancellationToken)
    {
        if (!Request.IsHttps || !await antiforgery.IsRequestValidAsync(HttpContext))
        {
            return Problem("The request could not be verified.", statusCode: StatusCodes.Status400BadRequest);
        }

        var dashboard = await dashboardService.CreateAsync(request, cancellationToken);
        return CreatedAtRoute("GetDashboardById", new { id = dashboard.Id }, dashboard);
    }

    /// <summary>Updates a dashboard only if its revision is current.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<DashboardResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync(
        Guid id, UpdateDashboardRequest request, CancellationToken cancellationToken)
    {
        if (!Request.IsHttps || !await antiforgery.IsRequestValidAsync(HttpContext))
        {
            return Problem("The request could not be verified.", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await dashboardService.UpdateAsync(id, request, cancellationToken);
        if (result.Conflict)
        {
            return Problem("The dashboard was changed by another user.", statusCode: StatusCodes.Status409Conflict);
        }

        return result.Dashboard is null ? NotFound() : Ok(result.Dashboard);
    }
}
