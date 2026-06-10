using HelpBoard.Abstractions.Repositories;
using HelpBoard.Abstractions.Services;
using HelpBoard.Contracts;
using Microsoft.Extensions.Logging;

namespace HelpBoard.Services.Dashboard;

/// <summary>Computes aggregate dashboard metrics from ticket data.</summary>
internal sealed class DashboardService(
    ITicketReader ticketReader,
    ILogger<DashboardService> logger) : IDashboardService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await ticketReader.GetAllAsync(cancellationToken).ConfigureAwait(false);

        var statusCounts = Enum.GetValues<TicketStatus>()
            .Select(status => new DashboardStatusCountResponse(
                status,
                tickets.Count(ticket => ticket.Status == status)))
            .ToList();

        var summary = new DashboardSummaryResponse(
            tickets.Count,
            statusCounts.Where(x => x.Status is TicketStatus.Open or TicketStatus.InProgress).Sum(x => x.Count),
            statusCounts.Where(x => x.Status is TicketStatus.Resolved or TicketStatus.Closed).Sum(x => x.Count),
            statusCounts,
            DateTimeOffset.UtcNow);

        logger.LogInformation(
            "Dashboard summary calculated with {TotalTickets} total tickets",
            summary.TotalTickets);

        return summary;
    }
}