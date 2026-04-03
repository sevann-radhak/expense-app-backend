using System.Text;
using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Endpoints;
using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddAppSettingsLocal();

builder.Services.Configure<InitialAdminOptions>(builder.Configuration.GetSection(InitialAdminOptions.SectionName));
builder.Services.Configure<SetupOptions>(builder.Configuration.GetSection(SetupOptions.SectionName));
builder.Services.Configure<DevDataOptions>(builder.Configuration.GetSection(DevDataOptions.SectionName));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Expense Tracker API",
            Version = "v1",
            Description = "Expense tracker API: auth (JWT), users, dev book seeding, health.",
        });
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme.",
        });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = [],
    });
});

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    _ = builder.Services.AddExpenseTrackerSqlServer(connectionString);
    _ = builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<ExpenseTrackerDbContext>()
        .AddDefaultTokenProviders();

    JwtOptions jwtFromConfig = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
    string signingKey = jwtFromConfig.SigningKey ?? string.Empty;
    if (signingKey.Length < 32)
    {
        if (builder.Environment.IsDevelopment())
        {
            signingKey = "Development-only-Jwt-Key-Minimum32Chars!";
            Console.WriteLine("Warning: Jwt:SigningKey not set; using Development-only default. Set Jwt__SigningKey for production-like local tests.");
        }
        else
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters when ConnectionStrings:DefaultConnection is set.");
        }
    }

    string issuer = string.IsNullOrWhiteSpace(jwtFromConfig.Issuer) ? "ExpenseTracker" : jwtFromConfig.Issuer;
    string audience = string.IsNullOrWhiteSpace(jwtFromConfig.Audience) ? "ExpenseTracker" : jwtFromConfig.Audience;
    int accessMinutes = jwtFromConfig.AccessTokenMinutes > 0 ? jwtFromConfig.AccessTokenMinutes : 120;
    _ = builder.Services.Configure<JwtOptions>(o =>
    {
        o.Issuer = issuer;
        o.Audience = audience;
        o.SigningKey = signingKey;
        o.AccessTokenMinutes = accessMinutes;
    });

    _ = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                ClockSkew = TimeSpan.FromMinutes(1),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };
        });

    _ = builder.Services.AddAuthorization();
    _ = builder.Services.AddSingleton<JwtTokenService>();
    _ = builder.Services.AddHostedService<IdentitySeedHostedService>();
}
else
{
    Console.WriteLine(
        "Warning: ConnectionStrings:DefaultConnection is not set. SQL, Identity, JWT routes, and dev book endpoints are disabled.");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        _ = policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

await app.EnsureCreatedAndMigratedAsync(connectionString).ConfigureAwait(false);

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API v1");
});

app.UseCors();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.MapGet("/api/health", () => Results.Json(new { status = "ok", service = "expense-tracker-api" }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/api/hello", () => Results.Json(new { message = "Hello, world!" }))
    .WithName("HelloWorld")
    .WithTags("Hello");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.MapAuthEndpoints();
    app.MapUsersEndpoints();
    IOptions<DevDataOptions> devOpts = app.Services.GetRequiredService<IOptions<DevDataOptions>>();
    app.MapDevBookEndpoints(devOpts);
}

app.Run();
