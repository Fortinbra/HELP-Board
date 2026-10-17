using System.Security.Claims;
using HelpBoard.Api.Authentication;

namespace HelpBoard.Api.Tests.Authentication;

public sealed class GoogleWorkspaceIdentityPolicyTests
{
    [Fact]
    public void TryAssignRole_VerifiedWorkspaceMember_AssignsViewerRole()
    {
        // Arrange
        var principal = CreatePrincipal("viewer@example.org", "example.org", "true");

        // Act
        var isEligible = GoogleWorkspaceIdentityPolicy.TryAssignRole(
            principal,
            "example.org",
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            out var assignedRole);

        // Assert
        Assert.True(isEligible);
        Assert.Equal("Viewer", assignedRole);
        Assert.Contains(principal.Claims, claim =>
            claim.Type == ClaimTypes.Role && claim.Value == "Viewer");
    }

    [Fact]
    public void TryAssignRole_ConfiguredAdmin_AssignsAdminRole()
    {
        // Arrange
        var principal = CreatePrincipal("admin@example.org", "example.org", "true");

        // Act
        var isEligible = GoogleWorkspaceIdentityPolicy.TryAssignRole(
            principal,
            "example.org",
            new HashSet<string>(["admin@example.org"], StringComparer.OrdinalIgnoreCase),
            out var assignedRole);

        // Assert
        Assert.True(isEligible);
        Assert.Equal("Admin", assignedRole);
    }

    [Theory]
    [InlineData("person@outside.org", "outside.org", "true")]
    [InlineData("person@example.org", "outside.org", "true")]
    [InlineData("person@example.org", "example.org", "false")]
    [InlineData("person@example.org", null, "true")]
    public void TryAssignRole_IneligibleIdentity_DoesNotAssignRole(
        string email,
        string? hostedDomain,
        string emailVerified)
    {
        // Arrange
        var principal = CreatePrincipal(email, hostedDomain, emailVerified);

        // Act
        var isEligible = GoogleWorkspaceIdentityPolicy.TryAssignRole(
            principal,
            "example.org",
            new HashSet<string>(StringComparer.OrdinalIgnoreCase),
            out var assignedRole);

        // Assert
        Assert.False(isEligible);
        Assert.Null(assignedRole);
        Assert.DoesNotContain(principal.Claims, claim => claim.Type == ClaimTypes.Role);
    }

    private static ClaimsPrincipal CreatePrincipal(
        string email,
        string? hostedDomain,
        string emailVerified)
    {
        var claims = new List<Claim>
        {
            new("email", email),
            new("email_verified", emailVerified)
        };
        if (hostedDomain is not null)
        {
            claims.Add(new Claim("hd", hostedDomain));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Google"));
    }
}
