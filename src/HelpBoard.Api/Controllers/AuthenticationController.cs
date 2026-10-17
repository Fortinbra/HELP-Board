using System.Security.Claims;
using HelpBoard.Api.Authentication;
using HelpBoard.Contracts;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpBoard.Api.Controllers;

/// <summary>Starts and ends sign-in sessions and reports the current user's access.</summary>
[ApiController]
[Route("auth")]
[Produces("application/json")]
public sealed class AuthenticationController(
    GoogleWorkspaceOptions googleOptions,
    IAntiforgery antiforgery) : ControllerBase
{
    /// <summary>Returns whether Google sign-in is available and the current session's identity.</summary>
    [AllowAnonymous]
    [HttpGet("status")]
    [ProducesResponseType<AuthenticationStatusResponse>(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var isAuthenticated = User.Identity?.IsAuthenticated is true;
        var response = new AuthenticationStatusResponse(
            googleOptions.IsConfigured,
            isAuthenticated,
            isAuthenticated ? User.FindFirst("name")?.Value : null,
            isAuthenticated ? User.FindFirst("email")?.Value : null,
            isAuthenticated ? User.FindFirst(ClaimTypes.Role)?.Value : null);

        return Ok(response);
    }

    /// <summary>Returns an antiforgery token for the sign-out operation.</summary>
    [AllowAnonymous]
    [HttpGet("antiforgery")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAntiforgeryToken()
    {
        if (!Request.IsHttps)
        {
            return BadRequest();
        }

        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { token = tokens.RequestToken });
    }

    /// <summary>Redirects the user to Google Workspace sign-in.</summary>
    [AllowAnonymous]
    [HttpGet("sign-in")]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult SignIn([FromQuery] string? returnUrl)
    {
        if (!googleOptions.IsConfigured)
        {
            return Problem(
                "Google Workspace sign-in is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        return Challenge(
            new AuthenticationProperties
            {
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : "/"
            },
            "Google");
    }

    /// <summary>Ends the current sign-in session.</summary>
    [Authorize]
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignOutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Request.IsHttps || !await antiforgery.IsRequestValidAsync(HttpContext))
        {
            return BadRequest();
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
