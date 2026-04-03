namespace ExpenseTracker.Api.Configuration;

public static class JwtStartup
{
    public static JwtOptions Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        JwtOptions o = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        int minLen = o.MinimumSigningKeyLength > 0 ? o.MinimumSigningKeyLength : 32;
        string key = o.SigningKey ?? string.Empty;
        if (key.Length < minLen)
        {
            if (environment.IsDevelopment())
            {
                key = o.DevelopmentFallbackSigningKey ?? string.Empty;
                if (key.Length < minLen)
                {
                    throw new InvalidOperationException(
                        $"Development: set Jwt:SigningKey or Jwt:DevelopmentFallbackSigningKey (min {minLen} characters) when SQL is enabled.");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Jwt:SigningKey must be at least {minLen} characters when ConnectionStrings:DefaultConnection is set.");
            }
        }

        o.SigningKey = key;
        if (string.IsNullOrWhiteSpace(o.Issuer))
        {
            o.Issuer = "ExpenseTracker";
        }

        if (string.IsNullOrWhiteSpace(o.Audience))
        {
            o.Audience = "ExpenseTracker";
        }

        if (o.AccessTokenMinutes <= 0)
        {
            o.AccessTokenMinutes = 120;
        }

        if (o.ClockSkewMinutes <= 0)
        {
            o.ClockSkewMinutes = 1;
        }

        return o;
    }
}
