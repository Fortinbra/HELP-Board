using System.ComponentModel.DataAnnotations;

namespace HelpBoard.Contracts;

/// <summary>Payload for creating a new help board ticket.</summary>
public sealed record CreateTicketRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(4000)] string Description,
    TicketPriority Priority,
    [Required, MaxLength(100)] string CreatedBy);
