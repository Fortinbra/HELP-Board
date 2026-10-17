using System.Net.Http;
using System.Text.Json;
using HelpBoard.Contracts;
using HelpBoard.Web.Services;
using Microsoft.AspNetCore.Components;

namespace HelpBoard.Web.Components;

public partial class AuthenticationPanel : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private AuthenticationStatusResponse? _status;
    private string? _errorMessage;
    private bool _isLoading = true;
    private bool _isSigningOut;

    [Inject]
    private IAuthenticationClient AuthenticationClient { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    [Inject]
    private ILogger<AuthenticationPanel> Logger { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        if (NavigationManager.Uri.Contains("authError=sign_in_failed", StringComparison.Ordinal))
        {
            _errorMessage = "Sign-in did not complete. Check the Workspace account and configuration, then try again.";
        }

        try
        {
            _status = await AuthenticationClient.GetStatusAsync(_cancellationTokenSource.Token);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Unable to load authentication status.");
            _errorMessage = "Sign-in status is unavailable. Please try again later.";
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The authentication status response was invalid.");
            _errorMessage = "Sign-in status is unavailable. Please try again later.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task SignOutAsync()
    {
        _isSigningOut = true;
        _errorMessage = null;

        try
        {
            await AuthenticationClient.SignOutAsync(_cancellationTokenSource.Token);
            NavigationManager.NavigateTo("/", forceLoad: true);
        }
        catch (HttpRequestException exception)
        {
            Logger.LogError(exception, "Unable to end the current authentication session.");
            _errorMessage = "Sign-out failed. Please try again.";
            _isSigningOut = false;
        }
        catch (JsonException exception)
        {
            Logger.LogError(exception, "The antiforgery token response was invalid.");
            _errorMessage = "Sign-out failed. Please try again.";
            _isSigningOut = false;
        }
        catch (InvalidOperationException exception)
        {
            Logger.LogError(exception, "The authentication response could not be processed.");
            _errorMessage = "Sign-out failed. Please try again.";
            _isSigningOut = false;
        }
    }

    private void StartSignIn()
    {
        NavigationManager.NavigateTo("/auth/sign-in", forceLoad: true);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }
}
