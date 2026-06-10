using HelpBoard.Contracts;

namespace HelpBoard.Abstractions.Domain;

/// <summary>Represents a help board ticket entity.</summary>
public sealed class Ticket
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Title { get; set; }

    public required string Description { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public TicketPriority Priority { get; set; }

    public required string CreatedBy { get; init; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
