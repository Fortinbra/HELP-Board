using HelpBoard.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace HelpBoard.Repositories.Tests;

public sealed class DependencyInjectionTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void AddRepositories_WithEmptyOrWhitespaceConnectionString_ThrowsInvalidOperationException(string connectionString)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var exception = Record.Exception(() => services.AddRepositories(connectionString));

        // Assert
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal("Connection string 'HelpBoard' is not configured.", exception.Message);
    }
}