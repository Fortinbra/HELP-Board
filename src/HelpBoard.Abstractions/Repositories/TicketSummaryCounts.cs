namespace HelpBoard.Abstractions.Repositories;

/// <summary>Aggregate ticket counts used by dashboard summary views.</summary>
public sealed record TicketSummaryCounts(
    int TotalCount,
    int OpenCount,
    int InProgressCount,
    int ResolvedCount,
    int ClosedCount);