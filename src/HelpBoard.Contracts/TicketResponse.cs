namespace HelpBoard.Contracts;

/// <summary>Represents a help board ticket returned from the API.</summary>
public sealed record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    string CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
