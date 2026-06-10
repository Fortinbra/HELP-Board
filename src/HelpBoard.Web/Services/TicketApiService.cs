using HelpBoard.Contracts;
using System.Net.Http.Json;

namespace HelpBoard.Web.Services;

/// <summary>HTTP client service for communicating with the HELP-Board API tickets endpoint.</summary>
public sealed class TicketApiService(HttpClient httpClient)
{
    private const string TicketsRoute = "api/tickets";

    /// <summary>Retrieves all tickets from the API.</summary>
    public async Task<IReadOnlyList<TicketResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await httpClient
            .GetFromJsonAsync<List<TicketResponse>>(TicketsRoute, cancellationToken)
            .ConfigureAwait(false);

        return tickets ?? [];
    }

    /// <summary>Retrieves a single ticket by identifier.</summary>
    public async Task<TicketResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await httpClient
            .GetFromJsonAsync<TicketResponse>($"{TicketsRoute}/{id}", cancellationToken)
            .ConfigureAwait(false);

    /// <summary>Creates a new ticket.</summary>
    public async Task<TicketResponse?> CreateAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .PostAsJsonAsync(TicketsRoute, request, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TicketResponse>(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Updates an existing ticket.</summary>
    public async Task<TicketResponse?> UpdateAsync(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .PutAsJsonAsync($"{TicketsRoute}/{id}", request, cancellationToken)
            .ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<TicketResponse>(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>Deletes a ticket by identifier.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient
            .DeleteAsync($"{TicketsRoute}/{id}", cancellationToken)
            .ConfigureAwait(false);

        return response.IsSuccessStatusCode;
    }
}
