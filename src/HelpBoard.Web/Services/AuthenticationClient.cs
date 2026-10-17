using System.Net.Http.Json;
using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

public sealed class AuthenticationClient(HttpClient httpClient) : IAuthenticationClient
{
    public async Task<AuthenticationStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        return await httpClient.GetFromJsonAsync<AuthenticationStatusResponse>(
                "auth/status",
                cancellationToken)
            ?? throw new InvalidOperationException("The authentication status response was empty.");
    }

    public async Task SignOutAsync(CancellationToken cancellationToken)
    {
        using var tokenResponse = await httpClient.GetAsync("auth/antiforgery", cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        var antiforgeryToken = await tokenResponse.Content
            .ReadFromJsonAsync<AntiforgeryTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("The antiforgery token response was empty.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "auth/sign-out");
        request.Headers.Add("X-CSRF-TOKEN", antiforgeryToken.Token);

        using var signOutResponse = await httpClient.SendAsync(request, cancellationToken);
        signOutResponse.EnsureSuccessStatusCode();
    }

    private sealed record AntiforgeryTokenResponse(string Token);
}
