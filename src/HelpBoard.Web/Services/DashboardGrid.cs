using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

/// <summary>Places widgets by pushing collisions downward in a deterministic order.</summary>
public static class DashboardGrid
{
    public static IReadOnlyList<DashboardWidget>? Place(
        IReadOnlyList<DashboardWidget> widgets, DashboardWidget changed)
    {
        if (!DashboardLayout.IsValidWidget(changed))
        {
            return null;
        }

        var placed = new List<DashboardWidget> { changed };
        foreach (var widget in widgets.Where(widget => widget.Id != changed.Id)
                     .OrderBy(widget => widget.Row).ThenBy(widget => widget.Column).ThenBy(widget => widget.Id))
        {
            var candidate = widget;
            while (placed.Any(existing => DashboardLayout.Overlaps(existing, candidate)))
            {
                candidate = candidate with { Row = candidate.Row + 1 };
                if (!DashboardLayout.IsValidWidget(candidate))
                {
                    return null;
                }
            }

            placed.Add(candidate);
        }

        return DashboardLayout.IsValid(placed) ? placed : null;
    }
}
