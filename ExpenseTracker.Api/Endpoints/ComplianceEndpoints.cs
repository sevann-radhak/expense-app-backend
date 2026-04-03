using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace ExpenseTracker.Api.Endpoints;

public static class ComplianceEndpoints
{
    public static void MapComplianceEndpoints(this WebApplication app)
    {
        RouteGroupBuilder g = app.MapGroup("/api/compliance").WithTags("Compliance");

        _ = g.MapPost(
                "/me/delete-account",
                DeleteMyAccountAsync)
            .RequireAuthorization()
            .WithSummary("Delete the signed-in account and all server-side book data (GDPR-style erasure).")
            .WithDescription(
                "Requires the current password. Not allowed for the only remaining SuperAdmin. " +
                "Book data is removed before the Identity user is deleted.");

        _ = g.MapGet(
                "/me/export-hint",
                ExportHintAsync)
            .RequireAuthorization()
            .WithSummary("Documents how to export book data (same payload as sync download).")
            .WithDescription("Returns a stable JSON hint pointing clients to GET /api/sync/book for full snapshot export.");
    }

    private static IResult ExportHintAsync()
    {
        return Results.Ok(new ExportHintResponse(
            "Use GET /api/sync/book with the same Bearer token to download the full book snapshot (v1).",
            "/api/sync/book"));
    }

    private static async Task<IResult> DeleteMyAccountAsync(
        DeleteMyAccountRequest body,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        DevBookDataService bookReset,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(body.Password))
        {
            return Results.BadRequest(new { error = "Password is required." });
        }

        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        ApplicationUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return Results.NotFound();
        }

        IList<string> roles = await userManager.GetRolesAsync(user).ConfigureAwait(false);
        if (roles.Contains(AppRoles.SuperAdmin))
        {
            int superCount = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin).ConfigureAwait(false) is { } list
                ? list.Count
                : 0;
            if (superCount <= 1)
            {
                return Results.Conflict(new
                {
                    error = "Cannot delete the only SuperAdmin account. Assign another SuperAdmin first or use a different flow.",
                });
            }
        }

        if (!await userManager.CheckPasswordAsync(user, body.Password).ConfigureAwait(false))
        {
            return Results.Unauthorized();
        }

        await bookReset.ResetUserBookAsync(userId, cancellationToken).ConfigureAwait(false);
        IdentityResult del = await userManager.DeleteAsync(user).ConfigureAwait(false);
        return !del.Succeeded ? Results.BadRequest(new { errors = del.Errors.Select(e => e.Description).ToArray() }) : Results.NoContent();
    }

    private sealed record ExportHintResponse(string Message, string DownloadPath);

    internal sealed class DeleteMyAccountRequest
    {
        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
}
