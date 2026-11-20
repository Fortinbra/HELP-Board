using HelpBoard.Contracts;
using HelpBoard.Web.Services;

namespace HelpBoard.Web.Tests.Services;

public sealed class DashboardGridTests
{
    [Fact]
    public void Place_WhenWidgetsCollide_PushesThemDownWithoutOverlap()
    {
        var first = Widget(0, 0);
        var second = Widget(2, 0);
        var third = Widget(2, 2);

        var result = DashboardGrid.Place([first, second, third], first with { Column = 2 });

        Assert.NotNull(result);
        Assert.Equal(0, result.Single(widget => widget.Id == first.Id).Row);
        Assert.Equal(2, result.Single(widget => widget.Id == second.Id).Row);
        Assert.Equal(4, result.Single(widget => widget.Id == third.Id).Row);
    }

    [Theory]
    [InlineData(-1, 0, 2, 2)]
    [InlineData(5, 0, 2, 2)]
    [InlineData(0, 0, 1, 2)]
    [InlineData(0, -1, 2, 2)]
    public void Place_OutsideGridOrBelowMinimum_RejectsPlacement(int column, int row, int width, int height)
    {
        var widget = Widget(0, 0);

        var result = DashboardGrid.Place([widget], widget with
        {
            Column = column, Row = row, Width = width, Height = height
        });

        Assert.Null(result);
    }

    private static DashboardWidget Widget(int column, int row)
        => new(Guid.NewGuid(), "Text", "Notes", "", column, row, 2, 2);
}
