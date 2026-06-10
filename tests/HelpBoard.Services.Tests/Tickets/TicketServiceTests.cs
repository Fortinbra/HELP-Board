using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using HelpBoard.Contracts;
using HelpBoard.Services.Tickets;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HelpBoard.Services.Tests.Tickets;

public sealed class TicketServiceTests
{
    private readonly Mock<ITicketReader> _readerMock = new();
    private readonly Mock<ITicketWriter> _writerMock = new();
    private readonly TicketService _sut;

    public TicketServiceTests()
    {
        _sut = new TicketService(_readerMock.Object, _writerMock.Object, NullLogger<TicketService>.Instance);
    }

    [Fact]
    public async Task GetTicketAsync_WithValidId_ReturnsTicketResponse()
    {
        // Arrange
        var ticket = BuildTicket();
        _readerMock.Setup(r => r.GetByIdAsync(ticket.Id, default)).ReturnsAsync(ticket);

        // Act
        var result = await _sut.GetTicketAsync(ticket.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Title, result.Title);
    }

    [Fact]
    public async Task GetTicketAsync_WithUnknownId_ReturnsNull()
    {
        // Arrange
        _readerMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Ticket?)null);

        // Act
        var result = await _sut.GetTicketAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllTicketsAsync_ReturnsAllMappedTickets()
    {
        // Arrange
        var tickets = new List<Ticket> { BuildTicket(), BuildTicket() };
        _readerMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync(tickets);

        // Act
        var result = await _sut.GetAllTicketsAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateTicketAsync_ValidRequest_SavesAndReturnsTicket()
    {
        // Arrange
        var request = new CreateTicketRequest("Test Title", "Test description", TicketPriority.Medium, "user@example.com");
        _writerMock.Setup(w => w.AddAsync(It.IsAny<Ticket>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.CreateTicketAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(TicketStatus.Open, result.Status);
        _writerMock.Verify(w => w.AddAsync(It.IsAny<Ticket>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateTicketAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.CreateTicketAsync(null!));
    }

    [Fact]
    public async Task UpdateTicketAsync_WithValidId_UpdatesAndReturnsTicket()
    {
        // Arrange
        var ticket = BuildTicket();
        var request = new UpdateTicketRequest("Updated Title", "Updated description", TicketStatus.InProgress, TicketPriority.High);
        _readerMock.Setup(r => r.GetByIdAsync(ticket.Id, default)).ReturnsAsync(ticket);
        _writerMock.Setup(w => w.UpdateAsync(It.IsAny<Ticket>(), default)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateTicketAsync(ticket.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(TicketStatus.InProgress, result.Status);
        _writerMock.Verify(w => w.UpdateAsync(It.IsAny<Ticket>(), default), Times.Once);
    }

    [Fact]
    public async Task UpdateTicketAsync_WithUnknownId_ReturnsNull()
    {
        // Arrange
        _readerMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Ticket?)null);
        var request = new UpdateTicketRequest("Title", "Desc", TicketStatus.Open, TicketPriority.Low);

        // Act
        var result = await _sut.UpdateTicketAsync(Guid.NewGuid(), request);

        // Assert
        Assert.Null(result);
        _writerMock.Verify(w => w.UpdateAsync(It.IsAny<Ticket>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteTicketAsync_WithValidId_DeletesAndReturnsTrue()
    {
        // Arrange
        var ticket = BuildTicket();
        _readerMock.Setup(r => r.GetByIdAsync(ticket.Id, default)).ReturnsAsync(ticket);
        _writerMock.Setup(w => w.DeleteAsync(ticket.Id, default)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteTicketAsync(ticket.Id);

        // Assert
        Assert.True(result);
        _writerMock.Verify(w => w.DeleteAsync(ticket.Id, default), Times.Once);
    }

    [Fact]
    public async Task DeleteTicketAsync_WithUnknownId_ReturnsFalse()
    {
        // Arrange
        _readerMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Ticket?)null);

        // Act
        var result = await _sut.DeleteTicketAsync(Guid.NewGuid());

        // Assert
        Assert.False(result);
        _writerMock.Verify(w => w.DeleteAsync(It.IsAny<Guid>(), default), Times.Never);
    }

    private static Ticket BuildTicket() =>
        new()
        {
            Title = "Sample ticket",
            Description = "Sample description",
            CreatedBy = "tester@example.com"
        };
}
