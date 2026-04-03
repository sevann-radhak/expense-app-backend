using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Endpoints;
using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Api.Services;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddAppSettingsLocal();

_ = builder.Services.AddProblemDetails();

builder.Services.Configure<OpenApiOptions>(builder.Configuration.GetSection(OpenApiOptions.SectionName));
builder.Services.Configure<ApiEndpointsOptions>(builder.Configuration.GetSection(ApiEndpointsOptions.SectionName));
builder.Services.Configure<AppCorsOptions>(builder.Configuration.GetSection(AppCorsOptions.SectionName));
builder.Services.Configure<InitialAdminOptions>(builder.Configuration.GetSection(InitialAdminOptions.SectionName));
builder.Services.Configure<SetupOptions>(builder.Configuration.GetSection(SetupOptions.SectionName));
builder.Services.Configure<DevDataOptions>(builder.Configuration.GetSection(DevDataOptions.SectionName));
builder.Services.Configure<EntraJwtOptions>(builder.Configuration.GetSection(EntraJwtOptions.SectionName));

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
    EntraJwtOptions entra = builder.Configuration.GetSection(EntraJwtOptions.SectionName).Get<EntraJwtOptions>() ?? new EntraJwtOptions();
    _ = builder.Services.AddMemoryCache();
    _ = builder.Services.AddSingleton<IJwtBlocklist, MemoryCacheJwtBlocklist>();

    _ = builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddExpenseTrackerJwtSchemes(jwt, entra);

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

_ = builder.Services.AddExpenseTrackerRateLimiter(builder.Environment);

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

app.ValidateProductionSafety();
app.ValidateProductionCors();

_ = app.UseExpenseTrackerSwaggerUi(openApi);

app.UseCors();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.UseAuthentication();
    app.UseAuthorization();
    _ = app.UseExpenseTrackerRateLimiter(app.Environment);
}

ApiEndpointsOptions apiEndpoints = app.Services.GetRequiredService<IOptions<ApiEndpointsOptions>>().Value;
app.MapGet(
        "/api/health",
        async Task<IResult> (IServiceProvider sp, IOptions<ApiEndpointsOptions> apiOptions, CancellationToken ct) =>
        {
            ApiEndpointsOptions opt = apiOptions.Value;
            string status = opt.HealthStatus;
            string database = "skipped";
            ExpenseTrackerDbContext? db = sp.GetService<ExpenseTrackerDbContext>();
            if (db is not null)
            {
                try
                {
                    bool ok = await db.Database.CanConnectAsync(ct).ConfigureAwait(false);
                    database = ok ? "ok" : "unavailable";
                    if (!ok)
                    {
                        status = "degraded";
                    }
                }
                catch
                {
                    database = "error";
                    status = "degraded";
                }
            }

            return Results.Json(new { status, service = opt.HealthServiceName, database });
        })
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/api/hello", () => Results.Json(new { message = apiEndpoints.HelloMessage }))
    .WithName("HelloWorld")
    .WithTags("Hello");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    app.MapAuthEndpoints(app.Environment);
    app.MapUsersEndpoints();
    app.MapSyncBookEndpoints(app.Environment);
    app.MapComplianceEndpoints();
    IOptions<DevDataOptions> devOpts = app.Services.GetRequiredService<IOptions<DevDataOptions>>();
    app.MapDevBookEndpoints(devOpts);
}

app.Run();

public partial class Program;
