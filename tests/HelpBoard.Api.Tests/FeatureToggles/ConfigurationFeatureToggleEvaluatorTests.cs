using HelpBoard.Api.FeatureToggles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace HelpBoard.Api.Tests.FeatureToggles;

public sealed class FeatureManagerMigrationFlagTests
{
    private const string MigrationFlag = FeatureFlagKeys.DatabaseMigrationsApplyAtStartup;

    [Fact]
    public async Task IsEnabledAsync_MissingFeatureManagementKey_ReturnsFalse()
    {
        // Arrange
        var configuration = BuildConfiguration([]);
        var sut = BuildFeatureManagerSnapshot(configuration);

        // Act
        var result = await sut.IsEnabledAsync(MigrationFlag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_InvalidFeatureValue_ReturnsFalse()
    {
        // Arrange
        var values = new Dictionary<string, string?>
        {
            [$"FeatureManagement:{MigrationFlag}"] = "not-a-bool"
        };
        var configuration = BuildConfiguration(values);
        var sut = BuildFeatureManagerSnapshot(configuration);

        // Act
        var result = await sut.IsEnabledAsync(MigrationFlag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_TrueFeatureValue_ReturnsTrue()
    {
        // Arrange
        var values = new Dictionary<string, string?>
        {
            [$"FeatureManagement:{MigrationFlag}"] = "true"
        };
        var configuration = BuildConfiguration(values);
        var sut = BuildFeatureManagerSnapshot(configuration);

        // Act
        var result = await sut.IsEnabledAsync(MigrationFlag);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_FalseFeatureValue_ReturnsFalse()
    {
        // Arrange
        var values = new Dictionary<string, string?>
        {
            [$"FeatureManagement:{MigrationFlag}"] = "false"
        };
        var configuration = BuildConfiguration(values);
        var sut = BuildFeatureManagerSnapshot(configuration);

        // Act
        var result = await sut.IsEnabledAsync(MigrationFlag);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_EnvironmentVariableStyleOverride_ReturnsTrue()
    {
        // Arrange
        var environmentVariableName = $"FeatureManagement__{FeatureFlagKeys.DatabaseMigrationsApplyAtStartup}";
        var previousValue = Environment.GetEnvironmentVariable(environmentVariableName);

        try
        {
            Environment.SetEnvironmentVariable(environmentVariableName, "true");

            var configuration = BuildConfiguration([], includeEnvironmentVariablesProvider: true);
            var sut = BuildFeatureManagerSnapshot(configuration);

            // Act
            var result = await sut.IsEnabledAsync(MigrationFlag);

            // Assert
            Assert.True(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(environmentVariableName, previousValue);
        }
    }

    private static IConfiguration BuildConfiguration(
        IEnumerable<KeyValuePair<string, string?>> values,
        bool includeEnvironmentVariablesProvider = false)
    {
        var builder = new ConfigurationBuilder()
            .AddInMemoryCollection(values);

        if (includeEnvironmentVariablesProvider)
        {
            builder.AddEnvironmentVariables();
        }

        return builder.Build();
    }

    private static IFeatureManagerSnapshot BuildFeatureManagerSnapshot(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddFeatureManagement();

        var serviceProvider = services.BuildServiceProvider();
        var featureManagerSnapshot = serviceProvider.GetRequiredService<IFeatureManagerSnapshot>();

        return featureManagerSnapshot;
    }
}
