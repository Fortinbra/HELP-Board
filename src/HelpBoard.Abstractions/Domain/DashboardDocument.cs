namespace HelpBoard.Abstractions.Domain;

/// <summary>Persisted shared dashboard configuration.</summary>
public sealed class DashboardDocument
{
    public Guid Id { get; init; }
    public required string Name { get; set; }
    public Guid Revision { get; set; }
    public required string WidgetsJson { get; set; }
}
