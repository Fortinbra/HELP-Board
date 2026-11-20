using HelpBoard.Abstractions.Domain;
using HelpBoard.Repositories.Dashboards;
using HelpBoard.Repositories.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HelpBoard.Repositories.Tests.Dashboards;

public sealed class DashboardRepositoryTests
{
    [Fact]
    public async Task UpdateAsync_ConcurrentEditors_RejectsStaleRevisionAndPreservesSavedContent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var setup = new AppDbContext(options);
        await setup.Database.EnsureCreatedAsync();
        var id = Guid.NewGuid();
        var revision = Guid.NewGuid();
        setup.Dashboards.Add(new DashboardDocument { Id = id, Name = "Original", Revision = revision, WidgetsJson = "[]" });
        await setup.SaveChangesAsync();
        await using var firstContext = new AppDbContext(options);
        await using var secondContext = new AppDbContext(options);
        var first = new DashboardRepository(firstContext, NullLogger<DashboardRepository>.Instance);
        var second = new DashboardRepository(secondContext, NullLogger<DashboardRepository>.Instance);
        var firstEdit = (await first.GetByIdAsync(id, default))!;
        var secondEdit = (await second.GetByIdAsync(id, default))!;
        firstEdit.Name = "First edit";
        firstEdit.Revision = Guid.NewGuid();
        secondEdit.Name = "Stale edit";
        secondEdit.Revision = Guid.NewGuid();

        var saved = await first.UpdateAsync(firstEdit, revision, default);
        var staleSaved = await second.UpdateAsync(secondEdit, revision, default);

        Assert.True(saved);
        Assert.False(staleSaved);
        setup.ChangeTracker.Clear();
        var stored = await setup.Dashboards.FindAsync(id);
        Assert.Equal("First edit", stored!.Name);
        Assert.Equal(firstEdit.Revision, stored.Revision);
    }
}
