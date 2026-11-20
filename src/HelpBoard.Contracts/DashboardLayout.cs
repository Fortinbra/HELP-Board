namespace HelpBoard.Contracts;

/// <summary>Validation rules for the shared dashboard grid.</summary>
public static class DashboardLayout
{
    public const int Columns = 6;
    public const int MinimumSize = 2;
    public const int MaximumHeight = 20;
    public const int MaximumRow = 1000;
    public const int MaximumWidgets = 50;

    public static bool IsValidWidget(DashboardWidget? widget)
        => widget is not null
           && widget.Id != Guid.Empty
           && widget.Type is "Line" or "Bar" or "Table" or "Text"
           && !string.IsNullOrWhiteSpace(widget.Title) && widget.Title.Length <= 100
           && widget.Content is not null && widget.Content.Length <= 4000
           && widget.Column >= 0 && widget.Row >= 0
           && widget.Width >= MinimumSize && widget.Width <= Columns
           && widget.Height >= MinimumSize && widget.Height <= MaximumHeight
           && widget.Column <= Columns - widget.Width
           && widget.Row <= MaximumRow - widget.Height;

    public static bool Overlaps(DashboardWidget left, DashboardWidget right)
        => left.Column < right.Column + right.Width && right.Column < left.Column + left.Width
           && left.Row < right.Row + right.Height && right.Row < left.Row + left.Height;

    public static bool IsValid(IReadOnlyList<DashboardWidget>? widgets)
    {
        if (widgets is null || widgets.Count > MaximumWidgets || widgets.Any(widget => !IsValidWidget(widget)))
        {
            return false;
        }

        if (widgets.Select(widget => widget.Id).Distinct().Count() != widgets.Count)
        {
            return false;
        }

        for (var index = 0; index < widgets.Count; index++)
        {
            for (var other = index + 1; other < widgets.Count; other++)
            {
                if (Overlaps(widgets[index], widgets[other]))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
