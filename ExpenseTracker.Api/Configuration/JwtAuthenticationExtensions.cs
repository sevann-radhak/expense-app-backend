using ExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ExpenseTracker.Api.Configuration;

public static class JwtAuthenticationExtensions
{
    public const string LocalJwtScheme = "LocalJwt";
    public const string EntraJwtScheme = "EntraJwt";

    public static AuthenticationBuilder AddExpenseTrackerJwtSchemes(this AuthenticationBuilder auth, JwtOptions jwt, EntraJwtOptions entra)
    {
        _ = auth.AddPolicyScheme(JwtBearerDefaults.AuthenticationScheme, JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (!entra.Enabled || string.IsNullOrWhiteSpace(entra.Authority))
                {
                    return LocalJwtScheme;
                }

                string? authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) ||
                    !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return LocalJwtScheme;
                }

                string token = authHeader["Bearer ".Length..].Trim();
                JwtSecurityTokenHandler handler = new();
                if (!handler.CanReadToken(token))
                {
                    return LocalJwtScheme;
                }

                JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
                return LooksLikeEntraIssuer(jwtToken.Issuer) ? EntraJwtScheme : LocalJwtScheme;
            };
        });

        _ = auth.AddJwtBearer(LocalJwtScheme, options => ConfigureLocalJwt(options, jwt));

        _ = entra.Enabled && !string.IsNullOrWhiteSpace(entra.Authority) && !string.IsNullOrWhiteSpace(entra.Audience)
            ? auth.AddJwtBearer(EntraJwtScheme, options =>
            {
                options.Authority = entra.Authority.TrimEnd('/');
                options.Audience = entra.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidAudience = entra.Audience,
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                };
            })
            : auth.AddJwtBearer(EntraJwtScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateLifetime = false,
                    RequireExpirationTime = false,
                    SignatureValidator = (_, _) => throw new SecurityTokenException("Entra JWT is not configured."),
                };
            });

        return auth;
    }

    private static void ConfigureLocalJwt(JwtBearerOptions options, JwtOptions jwt)
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(jwt.ClockSkewMinutes),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                string? jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrEmpty(jti))
                {
                    return Task.CompletedTask;
                }

                IJwtBlocklist blocklist = context.HttpContext.RequestServices.GetRequiredService<IJwtBlocklist>();
                if (blocklist.IsRevoked(jti))
                {
                    context.Fail("This token has been revoked.");
                }

                return Task.CompletedTask;
            },
        };
    }

    internal static bool LooksLikeEntraIssuer(string? issuer)
    {
        return !string.IsNullOrEmpty(issuer) && (issuer.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) ||
               issuer.Contains("sts.windows.net", StringComparison.OrdinalIgnoreCase) ||
               issuer.Contains("ciamlogin.com", StringComparison.OrdinalIgnoreCase));
    }
}
