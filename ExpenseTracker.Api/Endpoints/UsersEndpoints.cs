using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Api.Endpoints;

public static class UsersEndpoints
{
    private const string AdminAuditLoggerCategory = "ExpenseTracker.Api.AdminAudit";

    public static void MapUsersEndpoints(this WebApplication app)
    {
        RouteGroupBuilder g = app.MapGroup("/api/users").WithTags("Users");

        _ = g.MapGet("/me", GetMeAsync).RequireAuthorization();
        _ = g.MapPatch("/me", PatchMeAsync).RequireAuthorization();

        _ = g.MapPost(string.Empty, CreateUserAsync).RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));
        _ = g.MapGet(string.Empty, ListUsersAsync).RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));
        _ = g.MapGet("/{userId}", GetUserByIdAsync).RequireAuthorization();
        _ = g.MapPut("/{userId}", PutUserAsync).RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));
        _ = g.MapDelete("/{userId}", DeleteUserAsync).RequireAuthorization(policy => policy.RequireRole(AppRoles.Admin, AppRoles.SuperAdmin));
        _ = g.MapPut("/{userId}/roles", PutRolesAsync).RequireAuthorization(policy => policy.RequireRole(AppRoles.SuperAdmin));
    }

    private static async Task<IResult> GetMeAsync(ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
    {
        string? id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            return Results.Unauthorized();
        }

        ApplicationUser? user = await userManager.FindByIdAsync(id).ConfigureAwait(false);
        return user is null ? Results.NotFound() : Results.Ok(await ToUserDtoAsync(user, userManager).ConfigureAwait(false));
    }

    private static async Task<IResult> PatchMeAsync(
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        PatchMeRequest body)
    {
        string? id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(id))
        {
            return Results.Unauthorized();
        }

        ApplicationUser? user = await userManager.FindByIdAsync(id).ConfigureAwait(false);
        if (user is null)
        {
            return Results.NotFound();
        }

        if (body.DisplayName is not null)
        {
            user.DisplayName = string.IsNullOrWhiteSpace(body.DisplayName) ? null : body.DisplayName.Trim();
        }

        IdentityResult r = await userManager.UpdateAsync(user).ConfigureAwait(false);
        return !r.Succeeded
            ? Results.BadRequest(new { errors = r.Errors.Select(e => e.Description).ToArray() })
            : Results.Ok(await ToUserDtoAsync(user, userManager).ConfigureAwait(false));
    }

    private static async Task<IResult> CreateUserAsync(
        CreateUserRequest body,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest(new { error = "Email and password are required." });
        }

        string email = body.Email.Trim();
        if (await userManager.FindByEmailAsync(email).ConfigureAwait(false) is not null)
        {
            return Results.Conflict(new { error = "That email is already registered." });
        }

        List<string> roles = body.Roles is { Count: > 0 }
            ? body.Roles.Distinct(StringComparer.Ordinal).ToList()
            : [AppRoles.User];

        foreach (string role in roles)
        {
            if (!AppRoles.All.Contains(role))
            {
                return Results.BadRequest(new { error = $"Unknown role: {role}" });
            }
        }

        bool callerSuper = principal.IsInRole(AppRoles.SuperAdmin);
        if (!callerSuper && roles.Exists(r => r is AppRoles.SuperAdmin or AppRoles.Admin))
        {
            return Results.Forbid();
        }

        string tier = SubscriptionTierCodes.Basic;
        string tierSource = SubscriptionTierSourceCodes.Default;
        if (body.SubscriptionTier is not null)
        {
            if (!callerSuper)
            {
                return Results.Forbid();
            }

            string requested = body.SubscriptionTier.Trim();
            if (!SubscriptionTierCodes.IsValid(requested))
            {
                return Results.BadRequest(new { error = $"Unknown subscription tier: {body.SubscriptionTier}" });
            }

            tier = requested;
            tierSource = SubscriptionTierSourceCodes.Admin;
        }

        ApplicationUser user = new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = body.EmailConfirmed ?? false,
            DisplayName = string.IsNullOrWhiteSpace(body.DisplayName) ? null : body.DisplayName.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            SubscriptionTier = tier,
            SubscriptionTierSource = tierSource,
        };

        IdentityResult create = await userManager.CreateAsync(user, body.Password).ConfigureAwait(false);
        if (!create.Succeeded)
        {
            return Results.BadRequest(new { errors = create.Errors.Select(e => e.Description).ToArray() });
        }

        IdentityResult roleAdd = await userManager.AddToRolesAsync(user, roles).ConfigureAwait(false);
        if (!roleAdd.Succeeded)
        {
            _ = await userManager.DeleteAsync(user).ConfigureAwait(false);
            return Results.BadRequest(new { errors = roleAdd.Errors.Select(e => e.Description).ToArray() });
        }

        return Results.Created($"/api/users/{user.Id}", await ToUserDtoAsync(user, userManager).ConfigureAwait(false));
    }

    private static async Task<IResult> ListUsersAsync(
        UserManager<ApplicationUser> userManager,
        int page = 1,
        int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        int skip = (page - 1) * pageSize;
        int total = await userManager.Users.AsNoTracking().CountAsync().ConfigureAwait(false);
        List<ApplicationUser> items = await userManager.Users.AsNoTracking()
            .OrderBy(u => u.Email)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync()
            .ConfigureAwait(false);
        List<UserListItemDto> dtos = new();
        foreach (ApplicationUser u in items)
        {
            dtos.Add(await ToUserDtoAsync(u, userManager).ConfigureAwait(false));
        }

        return Results.Ok(new PagedUsersDto(page, pageSize, total, dtos));
    }

    private static async Task<IResult> GetUserByIdAsync(
        string userId,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager)
    {
        string? callerId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(callerId))
        {
            return Results.Unauthorized();
        }

        bool isAdmin = principal.IsInRole(AppRoles.Admin) || principal.IsInRole(AppRoles.SuperAdmin);
        if (!isAdmin && !string.Equals(callerId, userId, StringComparison.Ordinal))
        {
            return Results.Forbid();
        }

        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        return user is null ? Results.NotFound() : Results.Ok(await ToUserDtoAsync(user, userManager).ConfigureAwait(false));
    }

    private static async Task<IResult> PutUserAsync(
        string userId,
        PutUserRequest body,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ILoggerFactory loggerFactory)
    {
        ApplicationUser? target = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (target is null)
        {
            return Results.NotFound();
        }

        IList<string> targetRoles = await userManager.GetRolesAsync(target).ConfigureAwait(false);
        bool callerSuper = principal.IsInRole(AppRoles.SuperAdmin);
        bool callerAdmin = principal.IsInRole(AppRoles.Admin);
        if (targetRoles.Contains(AppRoles.SuperAdmin) && !callerSuper)
        {
            return Results.Forbid();
        }

        if (body.DisplayName is not null)
        {
            target.DisplayName = string.IsNullOrWhiteSpace(body.DisplayName) ? null : body.DisplayName.Trim();
        }

        if (body.LockoutEndUtc is not null && (callerSuper || callerAdmin))
        {
            if (body.LockoutEndUtc == DateTimeOffset.MinValue)
            {
                _ = await userManager.SetLockoutEndDateAsync(target, null).ConfigureAwait(false);
            }
            else
            {
                _ = await userManager.SetLockoutEndDateAsync(target, body.LockoutEndUtc).ConfigureAwait(false);
            }
        }

        if (body.SubscriptionTier is not null)
        {
            if (!callerSuper)
            {
                return Results.Forbid();
            }

            string requested = body.SubscriptionTier.Trim();
            if (!SubscriptionTierCodes.IsValid(requested))
            {
                return Results.BadRequest(new { error = $"Unknown subscription tier: {body.SubscriptionTier}" });
            }

            target.SubscriptionTier = requested;
            if (body.SubscriptionTierSource is not null)
            {
                string src = body.SubscriptionTierSource.Trim();
                if (!SubscriptionTierSourceCodes.IsValid(src))
                {
                    return Results.BadRequest(new { error = $"Unknown subscription tier source: {body.SubscriptionTierSource}" });
                }

                target.SubscriptionTierSource = src;
            }
            else
            {
                target.SubscriptionTierSource = SubscriptionTierSourceCodes.Admin;
            }
        }

        IdentityResult r = await userManager.UpdateAsync(target).ConfigureAwait(false);
        if (!r.Succeeded)
        {
            return Results.BadRequest(new { errors = r.Errors.Select(e => e.Description).ToArray() });
        }

        if (body.SubscriptionTier is not null)
        {
            ILogger log = loggerFactory.CreateLogger(AdminAuditLoggerCategory);
            string? actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            log.LogWarning(
                "AdminSubscriptionTierChanged: actorUserId={ActorId} targetUserId={TargetId} newTier={Tier}",
                actorId,
                userId,
                target.SubscriptionTier);
        }

        return Results.Ok(await ToUserDtoAsync(target, userManager).ConfigureAwait(false));
    }

    private static async Task<IResult> DeleteUserAsync(
        string userId,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ILoggerFactory loggerFactory)
    {
        ApplicationUser? target = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (target is null)
        {
            return Results.NotFound();
        }

        string? callerId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.Equals(callerId, userId, StringComparison.Ordinal))
        {
            return Results.BadRequest(new { error = "Use a support flow to delete your own account." });
        }

        IList<string> roles = await userManager.GetRolesAsync(target).ConfigureAwait(false);
        bool callerSuper = principal.IsInRole(AppRoles.SuperAdmin);
        if (roles.Contains(AppRoles.SuperAdmin) && !callerSuper)
        {
            return Results.Forbid();
        }

        if (!callerSuper && roles.Contains(AppRoles.Admin))
        {
            return Results.Forbid();
        }

        string? actorIdForAudit = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        IdentityResult r = await userManager.DeleteAsync(target).ConfigureAwait(false);
        if (!r.Succeeded)
        {
            return Results.BadRequest(new { errors = r.Errors.Select(e => e.Description).ToArray() });
        }

        ILogger auditLog = loggerFactory.CreateLogger(AdminAuditLoggerCategory);
        auditLog.LogWarning(
            "AdminUserDeleted: actorUserId={ActorId} targetUserId={TargetId} targetEmail={TargetEmail}",
            actorIdForAudit,
            userId,
            target.Email);
        return Results.NoContent();
    }

    private static async Task<IResult> PutRolesAsync(
        string userId,
        PutRolesRequest body,
        UserManager<ApplicationUser> userManager,
        ClaimsPrincipal principal,
        ILoggerFactory loggerFactory)
    {
        ApplicationUser? target = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (target is null)
        {
            return Results.NotFound();
        }

        if (body.Roles is null || body.Roles.Count == 0)
        {
            return Results.BadRequest(new { error = "At least one role is required." });
        }

        List<string> distinctRoles = body.Roles.Distinct(StringComparer.Ordinal).ToList();
        foreach (string role in distinctRoles)
        {
            if (!AppRoles.All.Contains(role))
            {
                return Results.BadRequest(new { error = $"Unknown role: {role}" });
            }
        }

        IList<string> current = await userManager.GetRolesAsync(target).ConfigureAwait(false);
        IdentityResult remove = await userManager.RemoveFromRolesAsync(target, current).ConfigureAwait(false);
        if (!remove.Succeeded)
        {
            return Results.BadRequest(new { errors = remove.Errors.Select(e => e.Description).ToArray() });
        }

        IdentityResult add = await userManager.AddToRolesAsync(target, distinctRoles).ConfigureAwait(false);
        if (!add.Succeeded)
        {
            return Results.BadRequest(new { errors = add.Errors.Select(e => e.Description).ToArray() });
        }

        ILogger log = loggerFactory.CreateLogger(AdminAuditLoggerCategory);
        string? actorId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        log.LogWarning(
            "AdminRolesReplaced: actorUserId={ActorId} targetUserId={TargetId} roles={Roles}",
            actorId,
            userId,
            string.Join(',', distinctRoles));
        return Results.Ok(await ToUserDtoAsync(target, userManager).ConfigureAwait(false));
    }

    private static async Task<UserListItemDto> ToUserDtoAsync(ApplicationUser user, UserManager<ApplicationUser> userManager)
    {
        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        return new UserListItemDto(
            user.Id,
            user.Email ?? "",
            user.DisplayName,
            user.EmailConfirmed,
            user.LockoutEnd,
            roles.ToList(),
            user.SubscriptionTier,
            user.SubscriptionTierSource);
    }

    internal sealed class CreateUserRequest
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("emailConfirmed")]
        public bool? EmailConfirmed { get; set; }

        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }

        [JsonPropertyName("subscriptionTier")]
        public string? SubscriptionTier { get; set; }
    }

    internal sealed class PatchMeRequest
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }
    }

    internal sealed class PutUserRequest
    {
        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("lockoutEndUtc")]
        public DateTimeOffset? LockoutEndUtc { get; set; }

        [JsonPropertyName("subscriptionTier")]
        public string? SubscriptionTier { get; set; }

        [JsonPropertyName("subscriptionTierSource")]
        public string? SubscriptionTierSource { get; set; }
    }

    internal sealed class PutRolesRequest
    {
        [JsonPropertyName("roles")]
        public List<string>? Roles { get; set; }
    }

    internal sealed record UserListItemDto(
        string Id,
        string Email,
        string? DisplayName,
        bool EmailConfirmed,
        DateTimeOffset? LockoutEnd,
        IReadOnlyList<string> Roles,
        string SubscriptionTier,
        string SubscriptionTierSource);

    internal sealed record PagedUsersDto(int Page, int PageSize, int TotalCount, IReadOnlyList<UserListItemDto> Items);
}
