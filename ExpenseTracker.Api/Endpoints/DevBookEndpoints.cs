using System.Text.Json.Serialization;
using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Infrastructure.Services;
using ExpenseTracker.Infrastructure.Taxonomy;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Endpoints;

public static class DevBookEndpoints
{
    private sealed class DevUserBody
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }

    public static void MapDevBookEndpoints(this WebApplication app, IOptions<DevDataOptions> devOptionsAccessor)
    {
        var devOptions = devOptionsAccessor.Value;
        if (!devOptions.ExposeEndpoints)
        {
            return;
        }

        var group = app.MapGroup("/api/dev/books").WithTags("Dev");

        group.MapPost(
                "/reset",
                async (HttpContext http, DevUserBody body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!ValidateSecret(devOptions, http))
                    {
                        return Results.Unauthorized();
                    }

                    if (string.IsNullOrWhiteSpace(body.UserId))
                    {
                        return Results.BadRequest(new { error = "userId is required." });
                    }

                    try
                    {
                        DevBookDataService.ValidateUserId(body.UserId);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }

                    await svc.ResetUserBookAsync(body.UserId, ct).ConfigureAwait(false);
                    return Results.Ok(new { status = "ok", message = "User book data removed." });
                })
            .WithSummary("Delete all book rows for a user (dev only).");

        group.MapPost(
                "/seed-taxonomy",
                async (HttpContext http, DevUserBody body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!ValidateSecret(devOptions, http))
                    {
                        return Results.Unauthorized();
                    }

                    if (string.IsNullOrWhiteSpace(body.UserId))
                    {
                        return Results.BadRequest(new { error = "userId is required." });
                    }

                    try
                    {
                        DevBookDataService.ValidateUserId(body.UserId);
                        await svc.SeedTaxonomyAsync(body.UserId, ct).ConfigureAwait(false);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Conflict(new { error = ex.Message });
                    }

                    return Results.Ok(new { status = "ok", message = "Taxonomy seeded.", templateId = TaxonomyConstants.DefaultTemplateId });
                })
            .WithSummary("Insert default expense + income taxonomy for a user (empty book).");

        group.MapPost(
                "/seed-demo",
                async (HttpContext http, DevUserBody body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!ValidateSecret(devOptions, http))
                    {
                        return Results.Unauthorized();
                    }

                    if (string.IsNullOrWhiteSpace(body.UserId))
                    {
                        return Results.BadRequest(new { error = "userId is required." });
                    }

                    try
                    {
                        DevBookDataService.ValidateUserId(body.UserId);
                        await svc.SeedDemoAsync(body.UserId, ct).ConfigureAwait(false);
                    }
                    catch (ArgumentException ex)
                    {
                        return Results.BadRequest(new { error = ex.Message });
                    }

                    return Results.Ok(new { status = "ok", message = "Demo book seeded (taxonomy + sample rows)." });
                })
            .WithSummary("Reset user, then taxonomy + demo payment instrument, expenses, and income (temporary dev helper).");
    }

    private static bool ValidateSecret(DevDataOptions devOptions, HttpContext http)
    {
        if (!devOptions.RequireSharedSecret || string.IsNullOrEmpty(devOptions.SharedSecret))
        {
            return true;
        }

        return http.Request.Headers.TryGetValue("X-Dev-Data-Secret", out var sent) &&
               sent.ToString() == devOptions.SharedSecret;
    }
}

