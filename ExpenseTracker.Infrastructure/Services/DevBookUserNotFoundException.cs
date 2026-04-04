namespace ExpenseTracker.Infrastructure.Services;

public sealed class DevBookUserNotFoundException(string userId) : Exception($"No application user exists with id '{userId}'. Register the user first or use a valid Identity user id.")
{
    public string UserId { get; } = userId;
}
