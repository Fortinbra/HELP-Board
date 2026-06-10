namespace HelpBoard.Contracts;

/// <summary>Represents aggregate dashboard KPI values for tickets.</summary>
public sealed record DashboardSummaryResponse(
    int TotalTickets,
    int OpenTickets,
    int ClosedTickets,
    IReadOnlyList<DashboardStatusCountResponse> StatusCounts,
    DateTimeOffset LastUpdatedUtc);