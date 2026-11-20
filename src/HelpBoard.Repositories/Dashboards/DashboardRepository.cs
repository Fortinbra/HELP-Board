using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Repositories.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Repositories.Dashboards;

internal sealed class DashboardRepository(AppDbContext dbContext, ILogger<DashboardRepository> logger)
    : IDashboardReader, IDashboardWriter
{
    public async Task<IReadOnlyList<DashboardDocument>> GetAllAsync(CancellationToken cancellationToken)
        => await dbContext.Dashboards.AsNoTracking().OrderBy(dashboard => dashboard.Name)
            .ThenBy(dashboard => dashboard.Id).ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<DashboardDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await dbContext.Dashboards.AsNoTracking()
            .FirstOrDefaultAsync(dashboard => dashboard.Id == id, cancellationToken).ConfigureAwait(false);

    public async Task AddAsync(DashboardDocument dashboard, CancellationToken cancellationToken)
    {
        await dbContext.Dashboards.AddAsync(dashboard, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(
        DashboardDocument dashboard, Guid expectedRevision, CancellationToken cancellationToken)
    {
        dbContext.Dashboards.Update(dashboard);
        dbContext.Entry(dashboard).Property(item => item.Revision).OriginalValue = expectedRevision;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(exception, "Concurrent update rejected for dashboard {DashboardId}.", dashboard.Id);
            return false;
        }
    }
}
