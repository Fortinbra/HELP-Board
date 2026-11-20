using System.Net.Http.Json;
using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

/// <summary>Calls the shared dashboard API using the current authenticated session.</summary>
public sealed class DashboardApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<DashboardResponse>> GetAllAsync(CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<List<DashboardResponse>>("api/dashboards", cancellationToken)
           ?? throw new InvalidOperationException("The dashboard list response was empty.");

    public async Task<DashboardResponse> GetAsync(Guid id, CancellationToken cancellationToken)
        => await httpClient.GetFromJsonAsync<DashboardResponse>($"api/dashboards/{id}", cancellationToken)
           ?? throw new InvalidOperationException("The dashboard response was empty.");

    public Task<DashboardResponse> CreateAsync(string name, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Post, "api/dashboards", new CreateDashboardRequest { Name = name }, cancellationToken);

    public Task<DashboardResponse> SaveAsync(DashboardResponse dashboard, CancellationToken cancellationToken)
        => SendAsync(HttpMethod.Put, $"api/dashboards/{dashboard.Id}", new UpdateDashboardRequest
        {
            Name = dashboard.Name, Revision = dashboard.Revision, Widgets = dashboard.Widgets
        }, cancellationToken);

    private async Task<DashboardResponse> SendAsync<T>(
        HttpMethod method, string uri, T body, CancellationToken cancellationToken)
    {
        var token = await httpClient.GetFromJsonAsync<AntiforgeryToken>("auth/antiforgery", cancellationToken)
            ?? throw new InvalidOperationException("The antiforgery token response was empty.");
        using var request = new HttpRequestMessage(method, uri) { Content = JsonContent.Create(body) };
        request.Headers.Add("X-CSRF-TOKEN", token.Token);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DashboardResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The dashboard response was empty.");
    }

    private sealed record AntiforgeryToken(string Token);
}
