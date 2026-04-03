using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseTracker.Api.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> optionsAccessor)
{
    private readonly JwtOptions _options = optionsAccessor.Value;

    public (string Token, DateTimeOffset ExpiresAtUtc) CreateAccessToken(ApplicationUser user, IReadOnlyList<string> roles)
    {
        if (string.IsNullOrWhiteSpace(_options.SigningKey) || _options.SigningKey.Length < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey must be set (min 32 characters). Use user secrets or environment variable Jwt__SigningKey.");
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_options.SigningKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];
        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        JwtSecurityToken token = new(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);
        string jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, new DateTimeOffset(expires, TimeSpan.Zero));
    }
}
