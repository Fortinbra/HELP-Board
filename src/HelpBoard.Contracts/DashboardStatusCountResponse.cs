namespace HelpBoard.Contracts;

/// <summary>Represents the number of tickets for a specific status.</summary>
public sealed record DashboardStatusCountResponse(
    TicketStatus Status,
    int Count);