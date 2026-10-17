namespace HelpBoard.Contracts;

/// <summary>Describes Google sign-in availability and the current application session.</summary>
public sealed record AuthenticationStatusResponse(
    bool GoogleSignInEnabled,
    bool IsAuthenticated,
    string? DisplayName,
    string? Email,
    string? Role);
