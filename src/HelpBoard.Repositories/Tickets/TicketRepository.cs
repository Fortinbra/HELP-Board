using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HelpBoard.Repositories.Tickets;

/// <summary>PostgreSQL-backed repository for <see cref="Ticket"/> entities.</summary>
internal sealed class TicketRepository(AppDbContext dbContext) : ITicketReader, ITicketWriter
{
    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
        => await dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        await dbContext.Tickets.AddAsync(ticket, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        dbContext.Tickets.Update(ticket);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await dbContext.Tickets
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            .ConfigureAwait(false);

        if (ticket is null)
        {
            return;
        }

        dbContext.Tickets.Remove(ticket);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
