using HelpBoard.Web.Services;
using Microsoft.FeatureManagement;
using Moq;

namespace HelpBoard.Web.Tests.Services;

public sealed class FeatureToggleServiceTests
{
    [Theory]
    [InlineData(FeatureToggleKeys.StrategicPlan, true)]
    [InlineData(FeatureToggleKeys.StrategicPlan, false)]
    [InlineData(FeatureToggleKeys.HelloWorldBaseline, true)]
    [InlineData(FeatureToggleKeys.HelloWorldBaseline, false)]
    [InlineData(FeatureToggleKeys.LightTheme, true)]
    [InlineData(FeatureToggleKeys.DarkTheme, true)]
    [InlineData(FeatureToggleKeys.HelpTheme, true)]
    [InlineData(FeatureToggleKeys.LightTheme, false)]
    [InlineData(FeatureToggleKeys.DarkTheme, false)]
    [InlineData(FeatureToggleKeys.HelpTheme, false)]
    public async Task IsEnabledAsync_KnownKey_ReturnsFeatureManagerValue(string key, bool expected)
    {
        // Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock
            .Setup(manager => manager.IsEnabledAsync(key))
            .ReturnsAsync(expected);

        var sut = new FeatureToggleService(featureManagerMock.Object);

        // Act
        var result = await sut.IsEnabledAsync(key);

        // Assert
        Assert.Equal(expected, result);
        featureManagerMock.Verify(manager => manager.IsEnabledAsync(key), Times.Once);
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownKey_ReturnsFalse()
    {
        // Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        var sut = new FeatureToggleService(featureManagerMock.Object);

        // Act
        var result = await sut.IsEnabledAsync("UnknownFeatureKey");

        // Assert
        Assert.False(result);
        featureManagerMock.Verify(manager => manager.IsEnabledAsync(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsEnabledAsync_NullOrWhitespaceKey_ReturnsFalse(string? featureKey)
    {
        // Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        var sut = new FeatureToggleService(featureManagerMock.Object);

        // Act
        var result = await sut.IsEnabledAsync(featureKey!);

        // Assert
        Assert.False(result);
        featureManagerMock.Verify(manager => manager.IsEnabledAsync(It.IsAny<string>()), Times.Never);
    }
}
