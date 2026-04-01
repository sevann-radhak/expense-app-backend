using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Api.Endpoints;
using ExpenseTracker.Api.Hosting;
using ExpenseTracker.Infrastructure;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddAppSettingsLocal();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new Microsoft.OpenApi.OpenApiInfo
        {
            Title = "Expense Tracker API",
            Version = "v1",
            Description = "Expense tracker HTTP API (health, dev book seeding, future sync).",
        });
});

builder.Services.Configure<DevDataOptions>(builder.Configuration.GetSection(DevDataOptions.SectionName));

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    _ = builder.Services.AddExpenseTrackerSqlServer(connectionString);
}
else
{
    Console.WriteLine(
        "Warning: ConnectionStrings:DefaultConnection is not set. SQL and dev book endpoints are disabled.");
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

app.MapGet("/api/health", () => Results.Json(new { status = "ok", service = "expense-tracker-api" }))
    .WithName("HealthCheck")
    .WithTags("System");

app.MapGet("/api/hello", () => Results.Json(new { message = "Hello, world!" }))
    .WithName("HelloWorld")
    .WithTags("Hello");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var devOpts = app.Services.GetRequiredService<IOptions<DevDataOptions>>();
    app.MapDevBookEndpoints(devOpts);
}

app.Run();
