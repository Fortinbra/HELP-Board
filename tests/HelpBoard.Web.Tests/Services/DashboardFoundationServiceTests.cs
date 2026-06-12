using HelpBoard.Web.Services;

namespace HelpBoard.Web.Tests.Services;

public sealed class DashboardFoundationServiceTests
{
    [Fact]
    public async Task GetFoundationSummaryAsync_ReturnsPlaceholderSummaryMetadata()
    {
        // Arrange
        var sut = new DashboardFoundationService();

        // Act
        var result = await sut.GetFoundationSummaryAsync();

        // Assert
        Assert.True(result.IsPlaceholderData);
        Assert.Contains("placeholder", result.DataSourceLabel, StringComparison.OrdinalIgnoreCase);
        Assert.True(result.LastUpdatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetFoundationSummaryAsync_ReturnsExpectedCardsAndWidgets()
    {
        // Arrange
        var sut = new DashboardFoundationService();

        // Act
        var result = await sut.GetFoundationSummaryAsync();

        // Assert
        Assert.Equal(4, result.Cards.Count);
        Assert.True(result.Widgets.Count >= 3);
        Assert.All(result.Widgets, widget => Assert.False(string.IsNullOrWhiteSpace(widget.Key)));
        Assert.All(result.Widgets, widget => Assert.False(string.IsNullOrWhiteSpace(widget.Title)));
        Assert.All(result.Widgets, widget => Assert.False(string.IsNullOrWhiteSpace(widget.WidgetTypeLabel)));
        Assert.All(result.Widgets, widget => Assert.False(string.IsNullOrWhiteSpace(widget.DrillDownRoute)));
    }

    [Fact]
    public async Task GetFoundationSummaryAsync_AllWidgetsContainMetricsWithNonColorTrendCues()
    {
        // Arrange
        var sut = new DashboardFoundationService();

        // Act
        var result = await sut.GetFoundationSummaryAsync();

        // Assert
        Assert.All(
            result.Widgets,
            widget =>
            {
                Assert.True(widget.Metrics.Count > 0);
                Assert.All(widget.Metrics, metric => Assert.False(string.IsNullOrWhiteSpace(metric.Label)));
                Assert.All(widget.Metrics, metric => Assert.False(string.IsNullOrWhiteSpace(metric.ValueText)));
                Assert.All(widget.Metrics, metric => Assert.False(string.IsNullOrWhiteSpace(metric.TrendLabel)));
                Assert.All(widget.Metrics, metric => Assert.False(string.IsNullOrWhiteSpace(metric.TrendIconName)));
            });
    }
}
