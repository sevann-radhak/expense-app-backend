namespace ExpenseTracker.Api.Services;

public interface IJwtBlocklist
{
    void Revoke(string jti, DateTimeOffset expiresAtUtc);

    bool IsRevoked(string jti);
}
