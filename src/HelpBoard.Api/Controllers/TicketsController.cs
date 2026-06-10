using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace HelpBoard.Api.Controllers;

/// <summary>Manages help board tickets.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class TicketsController(ITicketService ticketService) : ControllerBase
{
    /// <summary>Gets all tickets.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TicketResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var tickets = await ticketService.GetAllTicketsAsync(cancellationToken);
        return Ok(tickets);
    }

    /// <summary>Gets a specific ticket by identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.GetTicketAsync(id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    /// <summary>Creates a new ticket.</summary>
    [HttpPost]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.CreateTicketAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = ticket.Id }, ticket);
    }

    /// <summary>Updates an existing ticket.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateTicketRequest request, CancellationToken cancellationToken)
    {
        var ticket = await ticketService.UpdateTicketAsync(id, request, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    /// <summary>Deletes a ticket.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await ticketService.DeleteTicketAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
