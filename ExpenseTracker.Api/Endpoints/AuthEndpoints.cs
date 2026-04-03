using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app, IHostEnvironment environment)
    {
        RouteGroupBuilder g = app.MapGroup("/api/auth").WithTags("Auth");
        if (!environment.IsEnvironment("Integration"))
        {
            _ = g.RequireRateLimiting(RateLimiterExtensions.AuthPolicy);
        }

        _ = g.MapPost("/register", RegisterAsync).AllowAnonymous();
        _ = g.MapPost("/login", LoginAsync).AllowAnonymous();
        _ = g.MapPost("/logout", LogoutAsync).RequireAuthorization();
        _ = g.MapPost("/bootstrap-superadmin", BootstrapSuperAdminAsync).AllowAnonymous();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest body,
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required." });
        }

        string email = body.Email.Trim();
        ApplicationUser user = new()
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(body.DisplayName) ? null : body.DisplayName.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        IdentityResult create = await userManager.CreateAsync(user, body.Password).ConfigureAwait(false);
        if (!create.Succeeded)
        {
            return Results.BadRequest(new { errors = create.Errors.Select(e => e.Description).ToArray() });
        }

        IdentityResult roleAdd = await userManager.AddToRoleAsync(user, AppRoles.User).ConfigureAwait(false);
        if (!roleAdd.Succeeded)
        {
            _ = await userManager.DeleteAsync(user).ConfigureAwait(false);
            return Results.BadRequest(new { errors = roleAdd.Errors.Select(e => e.Description).ToArray() });
        }

        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        (string token, DateTimeOffset exp) = jwt.CreateAccessToken(user, roles.ToList());
        return Results.Created(
            $"/api/users/{user.Id}",
            new AuthResponseDto(user.Id, user.Email!, token, exp, roles.ToList()));
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest body,
        UserManager<ApplicationUser> userManager,
        JwtTokenService jwt,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required." });
        }

        ApplicationUser? user = await userManager.FindByEmailAsync(body.Email.Trim()).ConfigureAwait(false);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (await userManager.IsLockedOutAsync(user).ConfigureAwait(false))
        {
            return Results.Json(new { error = "Account is locked out." }, statusCode: StatusCodes.Status403Forbidden);
        }

        if (!await userManager.CheckPasswordAsync(user, body.Password).ConfigureAwait(false))
        {
            _ = await userManager.AccessFailedAsync(user).ConfigureAwait(false);
            return Results.Unauthorized();
        }

        _ = await userManager.ResetAccessFailedCountAsync(user).ConfigureAwait(false);
        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        (string token, DateTimeOffset exp) = jwt.CreateAccessToken(user, roles.ToList());
        return Results.Ok(new AuthResponseDto(user.Id, user.Email!, token, exp, roles.ToList()));
    }

    private static IResult LogoutAsync(HttpContext http, IJwtBlocklist blocklist)
    {
        string? jti = http.User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        string? expClaim = http.User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(expClaim) || !long.TryParse(expClaim, out long expUnix))
        {
            return Results.Unauthorized();
        }

        DateTimeOffset expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix);
        blocklist.Revoke(jti, expiresAt);
        return Results.NoContent();
    }

    private static async Task<IResult> BootstrapSuperAdminAsync(
        HttpContext http,
        BootstrapSuperAdminRequest body,
        UserManager<ApplicationUser> userManager,
        IOptions<SetupOptions> setupOptions,
        CancellationToken cancellationToken)
    {
        string? expected = setupOptions.Value.BootstrapToken;
        if (string.IsNullOrWhiteSpace(expected))
        {
            return Results.NotFound(new { error = "Bootstrap is not configured (Setup:BootstrapToken)." });
        }

        if (!http.Request.Headers.TryGetValue("X-Setup-Token", out var sent) || sent.ToString() != expected)
        {
            return Results.Unauthorized();
        }

        IList<ApplicationUser> supers = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin).ConfigureAwait(false);
        if (supers.Count > 0)
        {
            return Results.Conflict(new { error = "A SuperAdmin user already exists." });
        }

        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required." });
        }

        string email = body.Email.Trim();
        ApplicationUser? existing = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existing is not null)
        {
            return Results.Conflict(new { error = "That email is already registered." });
        }

        ApplicationUser user = new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow,
        };

        IdentityResult create = await userManager.CreateAsync(user, body.Password).ConfigureAwait(false);
        if (!create.Succeeded)
        {
            return Results.BadRequest(new { errors = create.Errors.Select(e => e.Description).ToArray() });
        }

        IdentityResult roleAdd = await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin).ConfigureAwait(false);
        if (!roleAdd.Succeeded)
        {
            _ = await userManager.DeleteAsync(user).ConfigureAwait(false);
            return Results.BadRequest(new { errors = roleAdd.Errors.Select(e => e.Description).ToArray() });
        }

        return Results.Ok(new { userId = user.Id, email = user.Email, roles = new[] { AppRoles.SuperAdmin } });
    }

    internal sealed class RegisterRequest
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }

    internal sealed class LoginRequest
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }

    internal sealed class BootstrapSuperAdminRequest
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }

    internal sealed record AuthResponseDto(
        string UserId,
        string Email,
        string AccessToken,
        DateTimeOffset ExpiresAtUtc,
        IReadOnlyList<string> Roles);
}
