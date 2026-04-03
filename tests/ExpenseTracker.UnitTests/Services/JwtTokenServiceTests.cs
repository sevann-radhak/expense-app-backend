using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static ExpenseTracker.Api.Configuration.JwtCustomClaimTypes;

namespace ExpenseTracker.UnitTests.Services;

[Trait("Category", "Unit")]
public sealed class JwtTokenServiceTests
{
    [Fact]
    public void CreateAccessToken_WithRoles_IncludesRoleClaims()
    {
        JwtOptions opts = new()
        {
            SigningKey = "Unit-Test-Jwt-Signing-Key-32chars!!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenMinutes = 60,
        };
        JwtTokenService service = new(Options.Create(opts));
        ApplicationUser user = new()
        {
            Id = "user-1",
            Email = "a@b.com",
            UserName = "a@b.com",
            SubscriptionTier = SubscriptionTierCodes.Pro,
        };

        (string token, DateTimeOffset exp) = service.CreateAccessToken(user, ["User", "Admin"]);

        _ = exp.Should().BeAfter(DateTimeOffset.UtcNow);
        JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        _ = jwt.Issuer.Should().Be("TestIssuer");
        _ = jwt.Audiences.Should().Contain("TestAudience");
        _ = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).Should().BeEquivalentTo(["User", "Admin"]);
        _ = jwt.Subject.Should().Be("user-1");
        _ = jwt.Claims.Single(c => c.Type == SubscriptionTier).Value.Should().Be(SubscriptionTierCodes.Pro);
    }

    [Fact]
    public void CreateAccessToken_ShortSigningKey_Throws()
    {
        JwtOptions opts = new() { SigningKey = "short", MinimumSigningKeyLength = 32 };
        JwtTokenService service = new(Options.Create(opts));
        ApplicationUser user = new() { Id = "1", Email = "x@y.z", UserName = "x@y.z" };

        Action act = () => service.CreateAccessToken(user, []);

        _ = act.Should().Throw<InvalidOperationException>().WithMessage("*SigningKey*");
    }
}
