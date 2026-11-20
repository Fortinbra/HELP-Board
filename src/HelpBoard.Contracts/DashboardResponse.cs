namespace HelpBoard.Contracts;

/// <summary>A shared dashboard and its current revision.</summary>
public sealed record DashboardResponse(Guid Id, string Name, Guid Revision, IReadOnlyList<DashboardWidget> Widgets);
