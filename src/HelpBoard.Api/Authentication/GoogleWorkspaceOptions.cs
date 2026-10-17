namespace HelpBoard.Api.Authentication;

/// <summary>Configuration required to authenticate members of a Google Workspace organization.</summary>
public sealed class GoogleWorkspaceOptions
{
    /// <summary>Configuration section containing the Google OAuth client settings.</summary>
    public const string SectionName = "Authentication:Google";

    /// <summary>Gets or sets the OAuth client identifier issued by Google.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Gets or sets the OAuth client secret issued by Google.</summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>Gets or sets the verified Google Workspace domain allowed to sign in.</summary>
    public string WorkspaceDomain { get; init; } = string.Empty;

    /// <summary>Gets or sets comma- or semicolon-separated addresses assigned the Admin role.</summary>
    public string AdminEmails { get; init; } = string.Empty;

    /// <summary>Gets a value indicating whether Google sign-in is configured.</summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.IsNullOrWhiteSpace(ClientSecret)
        && !string.IsNullOrWhiteSpace(WorkspaceDomain);

    /// <summary>Gets the configured administrator email addresses in normalized form.</summary>
    public IReadOnlySet<string> GetAdminEmails()
    {
        return AdminEmails
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(email => email.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
