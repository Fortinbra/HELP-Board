using HelpBoard.Abstractions.Domain;
using HelpBoard.Contracts;
using HelpBoard.Repositories.Data;
using HelpBoard.Repositories.Tickets;
using Microsoft.EntityFrameworkCore;

namespace HelpBoard.Repositories.Tests.Tickets;

public sealed class TicketRepositoryTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly TicketRepository _sut;

    public TicketRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _sut = new TicketRepository(_dbContext);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTicket_ReturnsTicket()
    {
        // Arrange
        var ticket = BuildTicket();
        await _dbContext.Tickets.AddAsync(ticket);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(ticket.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal(ticket.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithUnknownId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTickets()
    {
        // Arrange
        await _dbContext.Tickets.AddRangeAsync(BuildTicket(), BuildTicket(), BuildTicket());
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task AddAsync_PersistsTicket()
    {
        // Arrange
        var ticket = BuildTicket();

        // Act
        await _sut.AddAsync(ticket);

        // Assert
        var stored = await _dbContext.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal(ticket.Title, stored.Title);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        // Arrange
        var ticket = BuildTicket();
        await _dbContext.Tickets.AddAsync(ticket);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        ticket.Title = "Updated Title";
        ticket.Status = TicketStatus.Resolved;

        // Act
        await _sut.UpdateAsync(ticket);

        // Assert
        _dbContext.ChangeTracker.Clear();
        var stored = await _dbContext.Tickets.FindAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal("Updated Title", stored.Title);
        Assert.Equal(TicketStatus.Resolved, stored.Status);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTicket()
    {
        // Arrange
        var ticket = BuildTicket();
        await _dbContext.Tickets.AddAsync(ticket);
        await _dbContext.SaveChangesAsync();

        // Act
        await _sut.DeleteAsync(ticket.Id);

        // Assert
        var stored = await _dbContext.Tickets.FindAsync(ticket.Id);
        Assert.Null(stored);
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_DoesNotThrow()
    {
        // Act & Assert — should not throw
        var exception = await Record.ExceptionAsync(() => _sut.DeleteAsync(Guid.NewGuid()));
        Assert.Null(exception);
    }

    public void Dispose() => _dbContext.Dispose();

    private static Ticket BuildTicket() =>
        new()
        {
            Title = "Test ticket",
            Description = "Test description",
            CreatedBy = "tester@example.com"
        };
}
