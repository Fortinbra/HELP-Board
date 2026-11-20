using System.Net;
using System.Net.Http.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Services;

namespace HelpBoard.Web.Tests.Services;

public sealed class DashboardApiClientTests
{
    [Fact]
    public async Task SaveAsync_Conflict_SendsAntiforgeryTokenAndSurfacesFailure()
    {
        var handler = new ConflictHandler();
        var client = new DashboardApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") });
        var dashboard = new DashboardResponse(Guid.NewGuid(), "Board", Guid.NewGuid(), []);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.SaveAsync(dashboard, default));

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.True(handler.SawToken);
    }

    private sealed class ConflictHandler : HttpMessageHandler
    {
        public bool SawToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(new { token = "test-token" })
                });
            }

            SawToken = request.Headers.TryGetValues("X-CSRF-TOKEN", out var values)
                && values.Single() == "test-token";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict));
        }
    }
}
