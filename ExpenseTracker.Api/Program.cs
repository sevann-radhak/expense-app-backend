using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Endpoints;
using ExpenseTracker.Infrastructure;
using ExpenseTracker.Infrastructure.Database;
using ExpenseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Optional machine-specific secrets (gitignored). See appsettings.local.example.json.
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "appsettings.local.json"),
    optional: true,
    reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "Expense Tracker API",
            Version = "v1",
            Description =
                "HTTP API for sync, health, and dev-only book seeding. Pair with Flutter via AZURE_API_BASE_URL. " +
                "Phase 5.b: EF Core creates the database and applies migrations on startup when ConnectionStrings:DefaultConnection is set.",
        });
});

builder.Services.Configure<DevDataOptions>(builder.Configuration.GetSection(DevDataOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    _ = builder.Services.AddExpenseTrackerSqlServer(connectionString);
}
else
{
    Console.WriteLine(
        "Warning: ConnectionStrings:DefaultConnection is not set. SQL Server features and dev book endpoints are disabled. " +
        "Use user secrets or environment variables (see README).");
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        _ = policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    await SqlServerDatabaseCreator.EnsureDatabaseExistsAsync(connectionString).ConfigureAwait(false);
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetService<ExpenseTrackerDbContext>();
    if (db is not null)
    {
        await db.Database.MigrateAsync().ConfigureAwait(false);
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense Tracker API v1");
});

app.UseCors();

app.MapGet("/api/health", () => Results.Json(new { status = "ok", service = "expense-tracker-api" }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/api/hello", () => Results.Json(new { message = "Hello, world!" }))
    .WithName("HelloWorld")
    .WithTags("Hello");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var devOpts = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<DevDataOptions>>();
    app.MapDevBookEndpoints(devOpts);
}

app.Run();
