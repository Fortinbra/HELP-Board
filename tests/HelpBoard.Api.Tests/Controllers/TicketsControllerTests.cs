using System.Net;

namespace HelpBoard.Api.Tests.Controllers;

public sealed class TicketsControllerTests(HelpBoardWebApplicationFactory factory)
    : IClassFixture<HelpBoardWebApplicationFactory>
{
    [Fact]
    public async Task GetAll_ReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/tickets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/tickets/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
