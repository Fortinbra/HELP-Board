using HelpBoard.Abstractions.Domain;

namespace HelpBoard.Abstractions.Repositories;

/// <summary>Read operations for <see cref="Ticket"/> persistence.</summary>
public interface ITicketReader
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default);
}
