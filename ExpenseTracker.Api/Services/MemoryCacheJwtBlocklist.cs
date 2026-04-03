using Microsoft.Extensions.Caching.Memory;

namespace ExpenseTracker.Api.Services;

public sealed class MemoryCacheJwtBlocklist(IMemoryCache cache) : IJwtBlocklist
{
    private const string KeyPrefix = "jwt_revoked_";

    public void Revoke(string jti, DateTimeOffset expiresAtUtc)
    {
        if (string.IsNullOrEmpty(jti))
        {
            return;
        }

        DateTime utc = expiresAtUtc.UtcDateTime;
        if (utc <= DateTime.UtcNow)
        {
            return;
        }

        _ = cache.Set(
            KeyPrefix + jti,
            1,
            new MemoryCacheEntryOptions { AbsoluteExpiration = utc });
    }

    public bool IsRevoked(string jti)
    {
        return !string.IsNullOrEmpty(jti) && cache.TryGetValue(KeyPrefix + jti, out _);
    }
}
