using ExpenseTracker.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.UnitTests.Services;

[Trait("Category", "Unit")]
public sealed class MemoryCacheJwtBlocklistTests
{
    [Fact]
    public void Revoke_ThenIsRevoked_ReturnsTrue()
    {
        using MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));
        MemoryCacheJwtBlocklist blocklist = new(cache);
        DateTimeOffset exp = DateTimeOffset.UtcNow.AddMinutes(30);

        blocklist.Revoke("jti-abc", exp);

        _ = blocklist.IsRevoked("jti-abc").Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_UnknownJti_ReturnsFalse()
    {
        using MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));
        MemoryCacheJwtBlocklist blocklist = new(cache);

        _ = blocklist.IsRevoked("unknown").Should().BeFalse();
    }

    [Fact]
    public void Revoke_PastExpiration_DoesNotStore()
    {
        using MemoryCache cache = new(Options.Create(new MemoryCacheOptions()));
        MemoryCacheJwtBlocklist blocklist = new(cache);

        blocklist.Revoke("jti-old", DateTimeOffset.UtcNow.AddMinutes(-1));

        _ = blocklist.IsRevoked("jti-old").Should().BeFalse();
    }
}
