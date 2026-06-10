using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Services;

/// <summary>Business operations for managing help board tickets.</summary>
public interface ITicketService
{
    Task<TicketResponse?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketResponse>> GetAllTicketsAsync(CancellationToken cancellationToken = default);

    Task<TicketResponse> CreateTicketAsync(CreateTicketRequest request, CancellationToken cancellationToken = default);

    Task<TicketResponse?> UpdateTicketAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteTicketAsync(Guid id, CancellationToken cancellationToken = default);
}
