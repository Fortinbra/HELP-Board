using System.ComponentModel.DataAnnotations;

namespace HelpBoard.Contracts;

/// <summary>Payload for updating an existing help board ticket.</summary>
public sealed record UpdateTicketRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(4000)] string Description,
    TicketStatus Status,
    TicketPriority Priority);
