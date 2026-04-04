using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Infrastructure.Services;
using ExpenseTracker.Infrastructure.Taxonomy;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Endpoints;

public static class DevBookEndpoints
{
    public static void MapDevBookEndpoints(this WebApplication app, IOptions<DevDataOptions> devOptionsAccessor)
    {
        DevDataOptions devOptions = devOptionsAccessor.Value;
        if (!devOptions.ExposeEndpoints)
        {
            return;
        }

        RouteGroupBuilder group = app.MapGroup("/api/dev/books").WithTags("Dev");

        _ = group.MapPost(
                "/reset",
                async (HttpContext http, DevBookUserRequest body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!DevBookRequestValidation.TryValidate(http, devOptions, body, out var userId, out IResult? err))
                    {
                        return err!;
                    }

                    await svc.ResetUserBookAsync(userId, ct).ConfigureAwait(false);
                    return Results.Ok(new { status = "ok", message = "User book data removed." });
                })
            .WithSummary("Delete all book rows for a user (dev only).");

        _ = group.MapPost(
                "/seed-taxonomy",
                async (HttpContext http, DevBookUserRequest body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!DevBookRequestValidation.TryValidate(http, devOptions, body, out var userId, out IResult? err))
                    {
                        return err!;
                    }

                    try
                    {
                        await svc.SeedTaxonomyAsync(userId, ct).ConfigureAwait(false);
                    }
                    catch (DevBookUserNotFoundException ex)
                    {
                        return Results.NotFound(new { error = ex.Message });
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Conflict(new { error = ex.Message });
                    }

                    return Results.Ok(new
                    {
                        status = "ok",
                        message = "Taxonomy seeded.",
                        templateId = TaxonomyConstants.DefaultTemplateId,
                    });
                })
            .WithSummary("Insert default expense + income taxonomy for a user (empty book).");

        _ = group.MapPost(
                "/seed-demo",
                async (HttpContext http, DevBookUserRequest body, DevBookDataService svc, CancellationToken ct) =>
                {
                    if (!DevBookRequestValidation.TryValidate(http, devOptions, body, out var userId, out IResult? err))
                    {
                        return err!;
                    }

                    try
                    {
                        await svc.SeedDemoAsync(userId, ct).ConfigureAwait(false);
                    }
                    catch (DevBookUserNotFoundException ex)
                    {
                        return Results.NotFound(new { error = ex.Message });
                    }
                    catch (InvalidOperationException ex)
                    {
                        return Results.Conflict(new { error = ex.Message });
                    }

                    return Results.Ok(new { status = "ok", message = "Demo book seeded (taxonomy + sample rows)." });
                })
            .WithSummary("Reset user, then taxonomy + demo payment instrument, expenses, and income (temporary dev helper).");
    }
}
