using HelpBoard.Web.Services;
using Microsoft.FeatureManagement;
using Moq;

namespace HelpBoard.Web.Tests.Services;

public sealed class FeatureToggleServiceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsEnabledAsync_KnownKey_ReturnsFeatureManagerValue(bool expected)
    {
        // Arrange
        var featureManagerMock = new Mock<IFeatureManager>();
        featureManagerMock
            .Setup(manager => manager.IsEnabledAsync(FeatureToggleKeys.StrategicPlan))
            .ReturnsAsync(expected);

        var sut = new FeatureToggleService(featureManagerMock.Object);

        // Act
        var result = await sut.IsEnabledAsync(FeatureToggleKeys.StrategicPlan);

        // Assert
        Assert.Equal(expected, result);
        featureManagerMock.Verify(manager => manager.IsEnabledAsync(FeatureToggleKeys.StrategicPlan), Times.Once);
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
