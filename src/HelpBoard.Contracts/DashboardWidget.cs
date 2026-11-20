namespace HelpBoard.Contracts;

/// <summary>A widget and its position in a shared dashboard.</summary>
public sealed record DashboardWidget(
    Guid Id, string Type, string Title, string Content, int Column, int Row, int Width, int Height);
