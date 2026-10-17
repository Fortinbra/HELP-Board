using System.Security.Claims;

namespace HelpBoard.Api.Authentication;

/// <summary>Validates Workspace membership and assigns the application's initial access role.</summary>
public static class GoogleWorkspaceIdentityPolicy
{
    /// <summary>
    /// Confirms the verified Google identity belongs to the configured Workspace and assigns Viewer
    /// or Admin based on the configured administrator list.
    /// </summary>
    /// <param name="principal">The identity returned by Google's validated OpenID Connect flow.</param>
    /// <param name="workspaceDomain">The configured Workspace domain.</param>
    /// <param name="adminEmails">Email addresses permitted to receive the Admin role.</param>
    /// <param name="assignedRole">The role assigned when the identity is eligible.</param>
    /// <returns><see langword="true"/> when the identity is eligible; otherwise, <see langword="false"/>.</returns>
    public static bool TryAssignRole(
        ClaimsPrincipal principal,
        string workspaceDomain,
        IReadOnlySet<string> adminEmails,
        out string? assignedRole)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceDomain);
        ArgumentNullException.ThrowIfNull(adminEmails);

        assignedRole = null;
        var email = principal.FindFirst("email")?.Value;
        var hostedDomain = principal.FindFirst("hd")?.Value;
        var isEmailVerified = principal.FindFirst("email_verified")?.Value;
        var normalizedDomain = workspaceDomain.Trim().TrimStart('@');

        if (!string.Equals(isEmailVerified, "true", StringComparison.OrdinalIgnoreCase)
            || !IsInWorkspace(email, hostedDomain, normalizedDomain))
        {
            return false;
        }

        var identity = principal.Identities.FirstOrDefault(claimsIdentity => claimsIdentity.IsAuthenticated);
        if (identity is null)
        {
            return false;
        }

        assignedRole = adminEmails.Contains(email!)
            ? "Admin"
            : "Viewer";
        identity.AddClaim(new Claim(ClaimTypes.Role, assignedRole));
        return true;
    }

    private static bool IsInWorkspace(string? email, string? hostedDomain, string workspaceDomain)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(hostedDomain))
        {
            return false;
        }

        var emailDomainSeparator = email.LastIndexOf('@');
        if (emailDomainSeparator <= 0)
        {
            return false;
        }

        var emailDomain = email[(emailDomainSeparator + 1)..];
        return string.Equals(hostedDomain, workspaceDomain, StringComparison.OrdinalIgnoreCase)
            && string.Equals(emailDomain, workspaceDomain, StringComparison.OrdinalIgnoreCase);
    }
}
