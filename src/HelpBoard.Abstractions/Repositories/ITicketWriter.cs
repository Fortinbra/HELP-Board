using HelpBoard.Abstractions.Domain;

namespace HelpBoard.Abstractions.Repositories;

/// <summary>Write operations for <see cref="Ticket"/> persistence.</summary>
public interface ITicketWriter
{
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
