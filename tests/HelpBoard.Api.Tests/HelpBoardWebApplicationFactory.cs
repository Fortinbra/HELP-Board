using HelpBoard.Abstractions.Domain;
using HelpBoard.Abstractions.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HelpBoard.Api.Tests;

/// <summary>
/// Custom factory for API integration tests. Replaces the repository interfaces
/// with a lightweight in-memory fake so tests have no external database dependency.
/// EF Core behaviour is covered separately in HelpBoard.Repositories.Tests.
/// </summary>
public sealed class HelpBoardWebApplicationFactory : WebApplicationFactory<Program>
{
    internal interface ITestTicketStore
    {
        void Reset();

        Task SeedAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // ConfigureTestServices runs after the app's ConfigureServices,
        // so our registrations take effect last.
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITicketReader>();
            services.RemoveAll<ITicketWriter>();
            services.RemoveAll<ITestTicketStore>();

            var store = new InMemoryTicketStore();
            services.AddSingleton<ITicketReader>(store);
            services.AddSingleton<ITicketWriter>(store);
            services.AddSingleton<ITestTicketStore>(store);
        });
    }

    /// <summary>Simple thread-safe in-memory implementation of the ticket repository interfaces.</summary>
    private sealed class InMemoryTicketStore : ITicketReader, ITicketWriter, ITestTicketStore
    {
        private readonly List<Ticket> _tickets = [];

        public void Reset() => _tickets.Clear();

        public Task SeedAsync(IEnumerable<Ticket> tickets, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(tickets);

            _tickets.AddRange(tickets);
            return Task.CompletedTask;
        }

        public Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tickets.FirstOrDefault(t => t.Id == id));

        public Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Ticket>>(_tickets.AsReadOnly());

        public Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            _tickets.Add(ticket);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
        {
            var index = _tickets.FindIndex(t => t.Id == ticket.Id);
            if (index >= 0)
            {
                _tickets[index] = ticket;
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _tickets.RemoveAll(t => t.Id == id);
            return Task.CompletedTask;
        }
    }
}
