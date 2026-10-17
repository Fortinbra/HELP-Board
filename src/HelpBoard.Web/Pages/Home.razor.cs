using System.Net.Http;
using System.Text.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Pages;

public partial class Home : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private AuthenticationStatusResponse? _status;
    private string? _errorMessage;
    private bool _isLoading = true;

    [Inject]
    private IAuthenticationClient AuthenticationClient { get; set; } = null!;

    [Inject]
    private ILogger<Home> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _status = await AuthenticationClient.GetStatusAsync(_cancellationTokenSource.Token);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Unable to load authentication status for the public landing page.");
            _errorMessage = "Sign-in is temporarily unavailable. Please try again later.";
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The authentication status response was invalid.");
            _errorMessage = "Sign-in is temporarily unavailable. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void StartSignIn()
    {
        NavigationManager.NavigateTo("/auth/sign-in?returnUrl=%2Fdashboard", forceLoad: true);
    }

    private void OpenDashboard()
    {
        NavigationManager.NavigateTo("/dashboard", forceLoad: true);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
