using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Services.Tickets;

/// <summary>Handles business logic for help board tickets.</summary>
internal sealed class TicketService(
    ITicketReader ticketReader,
    ITicketWriter ticketWriter,
    ILogger<TicketService> logger) : ITicketService
{
    public async Task<TicketResponse?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketReader.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (ticket is null)
        {
            logger.LogInformation("Ticket {TicketId} was not found", id);
            return null;
        }

        return MapToResponse(ticket);
    }

    public async Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await ticketReader.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return tickets.Select(MapToResponse).ToList();
    }

    public async Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ticket = new Ticket
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            CreatedBy = request.CreatedBy
        };

        await ticketWriter.AddAsync(ticket, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Ticket {TicketId} created by {CreatedBy}", ticket.Id, ticket.CreatedBy);

        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var ticket = await ticketReader.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (ticket is null)
        {
            logger.LogInformation("Update failed — ticket {TicketId} was not found", id);
            return null;
        }

        ticket.Title = request.Title;
        ticket.Description = request.Description;
        ticket.Status = request.Status;
        ticket.Priority = request.Priority;
        ticket.UpdatedAt = DateTimeOffset.UtcNow;

        await ticketWriter.UpdateAsync(ticket, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Ticket {TicketId} updated", ticket.Id);

        return MapToResponse(ticket);
    }

    public async Task<bool> DeleteTicketAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketReader.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        await ticketWriter.DeleteAsync(id, cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Ticket {TicketId} deleted", id);

        return true;
    }

    private static TicketResponse MapToResponse(Ticket ticket) =>
        new(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedBy,
            ticket.CreatedAt,
            ticket.UpdatedAt);
}
