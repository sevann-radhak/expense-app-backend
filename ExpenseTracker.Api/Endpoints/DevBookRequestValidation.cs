using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.Extensions.Primitives;

namespace ExpenseTracker.Api.Endpoints;

internal static class DevBookRequestValidation
{
    internal static bool TryValidate(
        HttpContext http,
        DevDataOptions options,
        DevBookUserRequest body,
        out string userId,
        out IResult? error)
    {
        userId = string.Empty;
        error = null;

        if (!IsSharedSecretValid(options, http))
        {
            error = Results.Unauthorized();
            return false;
        }

        if (string.IsNullOrWhiteSpace(body.UserId))
        {
            error = Results.BadRequest(new { error = "userId is required." });
            return false;
        }

        try
        {
            DevBookDataService.ValidateUserId(body.UserId);
        }
        catch (ArgumentException ex)
        {
            error = Results.BadRequest(new { error = ex.Message });
            return false;
        }

        userId = body.UserId;
        return true;
    }

    private static bool IsSharedSecretValid(DevDataOptions options, HttpContext http)
    {
        return !options.RequireSharedSecret || string.IsNullOrEmpty(options.SharedSecret) || (http.Request.Headers.TryGetValue("X-Dev-Data-Secret", out StringValues sent) &&
               sent.ToString() == options.SharedSecret);
    }
}
