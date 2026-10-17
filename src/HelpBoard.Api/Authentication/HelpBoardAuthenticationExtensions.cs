using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace HelpBoard.Api.Authentication;

/// <summary>Registers HELP-Board's cookie session and optional Google Workspace sign-in.</summary>
public static class HelpBoardAuthenticationExtensions
{
    /// <summary>Registers secure cookie authentication and Google Workspace OIDC when configured.</summary>
    /// <param name="services">The application's service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHelpBoardAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var googleOptions = configuration.GetSection(GoogleWorkspaceOptions.SectionName)
            .Get<GoogleWorkspaceOptions>() ?? new GoogleWorkspaceOptions();
        ValidateGoogleConfiguration(googleOptions);
        services.AddSingleton(googleOptions);
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "__Host-HelpBoard.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        });

        var authentication = services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "__Host-HelpBoard.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.Path = "/";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = false;
                options.Events.OnRedirectToLogin = context =>
                {
                    if (IsApiOrAuthenticationRequest(context.Request.Path))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (IsApiOrAuthenticationRequest(context.Request.Path))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

        if (googleOptions.IsConfigured)
        {
            authentication.AddOpenIdConnect("Google", options =>
            {
                options.Authority = "https://accounts.google.com";
                options.ClientId = googleOptions.ClientId;
                options.ClientSecret = googleOptions.ClientSecret;
                options.CallbackPath = "/signin-google";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.SaveTokens = false;
                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = ClaimTypes.Role
                };
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Events.OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    if (context.Principal is null
                        || !GoogleWorkspaceIdentityPolicy.TryAssignRole(
                            context.Principal,
                            googleOptions.WorkspaceDomain,
                            googleOptions.GetAdminEmails(),
                            out var role))
                    {
                        logger.LogWarning(
                            "Rejected Google sign-in because the identity is not a verified member of the configured Workspace.");
                        context.Fail("Google Workspace membership could not be verified.");
                        return Task.CompletedTask;
                    }

                    logger.LogInformation("Google Workspace sign-in validated with role {Role}.", role);
                    return Task.CompletedTask;
                };
                options.Events.OnRemoteFailure = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(context.Failure, "Google Workspace sign-in did not complete.");
                    context.HandleResponse();
                    context.Response.Redirect("/?authError=sign_in_failed");
                    return Task.CompletedTask;
                };
            });
        }

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

        return services;
    }

    private static void ValidateGoogleConfiguration(GoogleWorkspaceOptions options)
    {
        var hasAnyConfiguration = !string.IsNullOrWhiteSpace(options.ClientId)
            || !string.IsNullOrWhiteSpace(options.ClientSecret)
            || !string.IsNullOrWhiteSpace(options.WorkspaceDomain)
            || !string.IsNullOrWhiteSpace(options.AdminEmails);

        if (!hasAnyConfiguration)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.ClientId)
            || string.IsNullOrWhiteSpace(options.ClientSecret)
            || string.IsNullOrWhiteSpace(options.WorkspaceDomain))
        {
            throw new InvalidOperationException(
                $"Google sign-in requires ClientId, ClientSecret, and WorkspaceDomain under '{GoogleWorkspaceOptions.SectionName}'.");
        }

        var workspaceDomain = options.WorkspaceDomain.Trim().TrimStart('@');
        if (Uri.CheckHostName(workspaceDomain) != UriHostNameType.Dns
            || !workspaceDomain.Contains('.', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The configured Google Workspace domain is invalid.");
        }

        var invalidAdminEmail = options.GetAdminEmails()
            .FirstOrDefault(email => email.IndexOf('@') <= 0
                || email.IndexOf('@') != email.LastIndexOf('@')
                || !email.EndsWith($"@{workspaceDomain}", StringComparison.OrdinalIgnoreCase));
        if (invalidAdminEmail is not null)
        {
            throw new InvalidOperationException(
                "Every configured administrator email must belong to the configured Google Workspace domain.");
        }
    }

    private static bool IsApiOrAuthenticationRequest(PathString path)
    {
        return path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/auth", StringComparison.OrdinalIgnoreCase);
    }
}
