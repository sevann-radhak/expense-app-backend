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

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddAppSettingsLocal();

builder.Services.Configure<OpenApiOptions>(builder.Configuration.GetSection(OpenApiOptions.SectionName));
builder.Services.Configure<ApiEndpointsOptions>(builder.Configuration.GetSection(ApiEndpointsOptions.SectionName));
builder.Services.Configure<AppCorsOptions>(builder.Configuration.GetSection(AppCorsOptions.SectionName));
builder.Services.Configure<InitialAdminOptions>(builder.Configuration.GetSection(InitialAdminOptions.SectionName));
builder.Services.Configure<SetupOptions>(builder.Configuration.GetSection(SetupOptions.SectionName));
builder.Services.Configure<DevDataOptions>(builder.Configuration.GetSection(DevDataOptions.SectionName));

OpenApiOptions openApi = builder.Configuration.GetSection(OpenApiOptions.SectionName).Get<OpenApiOptions>() ?? new OpenApiOptions();
_ = builder.Services.AddExpenseTrackerOpenApi(openApi);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    _ = builder.Services.AddExpenseTrackerSqlServer(connectionString);
    IConfigurationSection identitySection = builder.Configuration.GetSection("Identity");
    _ = builder.Services
        .AddIdentity<ApplicationUser, IdentityRole>(options => identitySection.Bind(options))
        .AddEntityFrameworkStores<ExpenseTrackerDbContext>()
        .AddDefaultTokenProviders();

    JwtOptions jwt = JwtStartup.Resolve(builder.Configuration, builder.Environment);
    _ = builder.Services.AddSingleton(Options.Create(jwt));

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
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                ClockSkew = TimeSpan.FromMinutes(jwt.ClockSkewMinutes),
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            };
        });

    _ = builder.Services.AddAuthorization();
    _ = builder.Services.AddSingleton<JwtTokenService>();
    _ = builder.Services.AddHostedService<IdentitySeedHostedService>();
}
else
{
    ApiEndpointsOptions apiWhenDisabled =
        builder.Configuration.GetSection(ApiEndpointsOptions.SectionName).Get<ApiEndpointsOptions>() ?? new ApiEndpointsOptions();
    if (apiWhenDisabled.LogWhenDatabaseDisabled)
    {
        Console.WriteLine(
            "Warning: ConnectionStrings:DefaultConnection is not set. SQL, Identity, JWT routes, and dev book endpoints are disabled.");
    }
}

AppCorsOptions cors = builder.Configuration.GetSection(AppCorsOptions.SectionName).Get<AppCorsOptions>() ?? new AppCorsOptions();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (cors.AllowAnyOrigin)
        {
            _ = policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
        }
        else if (cors.AllowedOrigins is { Length: > 0 })
        {
            _ = policy.WithOrigins(cors.AllowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            _ = policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

WebApplication app = builder.Build();

await app.EnsureCreatedAndMigratedAsync(connectionString).ConfigureAwait(false);

_ = app.UseExpenseTrackerSwaggerUi(openApi);

app.UseCors();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.UseAuthentication();
    app.UseAuthorization();
}

ApiEndpointsOptions apiEndpoints = app.Services.GetRequiredService<IOptions<ApiEndpointsOptions>>().Value;
app.MapGet(
        "/api/health",
        () => Results.Json(new { status = apiEndpoints.HealthStatus, service = apiEndpoints.HealthServiceName }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/api/hello", () => Results.Json(new { message = apiEndpoints.HelloMessage }))
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
