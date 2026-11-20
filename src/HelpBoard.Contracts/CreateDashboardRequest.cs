using System.ComponentModel.DataAnnotations;

namespace HelpBoard.Contracts;

/// <summary>Request to create a shared dashboard.</summary>
public sealed record CreateDashboardRequest
{
    [Required, StringLength(100)]
    public required string Name { get; init; }
}
