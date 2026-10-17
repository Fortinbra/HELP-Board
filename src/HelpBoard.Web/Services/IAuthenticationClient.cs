using HelpBoard.Contracts;

namespace HelpBoard.Web.Services;

public interface IAuthenticationClient
{
    /// <summary>Gets the current session's authentication status.</summary>
    Task<AuthenticationStatusResponse> GetStatusAsync(CancellationToken cancellationToken);

    /// <summary>Ends the current session after validating its antiforgery token.</summary>
    Task SignOutAsync(CancellationToken cancellationToken);
}
