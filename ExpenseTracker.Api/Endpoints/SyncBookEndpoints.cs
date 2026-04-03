using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Infrastructure.Services;
using ExpenseTracker.Infrastructure.Sync;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ExpenseTracker.Api.Endpoints;

public static class SyncBookEndpoints
{
    public static void MapSyncBookEndpoints(this WebApplication app, IHostEnvironment environment)
    {
        RouteGroupBuilder g = app.MapGroup("/api/sync/book").WithTags("Sync").RequireAuthorization();
        if (!environment.IsEnvironment("Integration"))
        {
            _ = g.RequireRateLimiting(RateLimiterExtensions.SyncPolicy);
        }

        _ = g.MapGet(
                "/",
                async Task<Results<Ok<BookSyncGetResponse>, UnauthorizedHttpResult>> (
                    ClaimsPrincipal user,
                    BookSyncService sync,
                    CancellationToken ct) =>
                {
                    string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        return TypedResults.Unauthorized();
                    }

                    BookSyncGetResponse doc = await sync.GetAsync(userId, ct).ConfigureAwait(false);
                    return TypedResults.Ok(doc);
                })
            .WithSummary("Download the authenticated user's book snapshot (full replace v1).")
            .WithDescription(
                "Returns `bookRevision` plus the same JSON shape as the Flutter book backup (schemaVersion 9). " +
                "Use `expectedBookRevision` on PUT for optimistic concurrency.");

        _ = g.MapPut(
                "/",
                async Task<Results<Ok<BookSyncPutResponse>, BadRequest<ProblemDetails>, Conflict<ProblemDetails>,
                    UnauthorizedHttpResult>> (
                    ClaimsPrincipal user,
                    [FromBody] PutBookSyncRequest body,
                    BookSyncService sync,
                    CancellationToken ct) =>
                {
                    string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        return TypedResults.Unauthorized();
                    }

                    BookSyncReplaceResult result = await sync.TryReplaceAsync(userId, body, ct).ConfigureAwait(false);
                    if (result.Success)
                    {
                        return TypedResults.Ok(new BookSyncPutResponse { BookRevision = result.NewBookRevision });
                    }

                    if (result.IsConflict)
                    {
                        ProblemDetails pd = new()
                        {
                            Status = StatusCodes.Status409Conflict,
                            Title = "Book revision conflict",
                            Detail = "The book changed on the server. Pull the latest snapshot and retry.",
                            Type = "https://httpstatuses.io/409",
                            Extensions =
                            {
                                ["code"] = "book_revision_conflict",
                                ["currentBookRevision"] = result.CurrentBookRevision,
                            },
                        };
                        return TypedResults.Conflict(pd);
                    }

                    ProblemDetails bad = new()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid book snapshot",
                        Detail = result.BadRequestDetail ?? "Validation failed.",
                        Type = "https://httpstatuses.io/400",
                        Extensions = { ["code"] = "book_snapshot_invalid" },
                    };
                    return TypedResults.BadRequest(bad);
                })
            .WithSummary("Replace the authenticated user's book with a full snapshot (v1).")
            .WithDescription(
                "Requires `expectedBookRevision` to match the server. On success returns the new `bookRevision`.");
    }
}

public sealed class BookSyncPutResponse
{
    public int BookRevision { get; set; }
}
