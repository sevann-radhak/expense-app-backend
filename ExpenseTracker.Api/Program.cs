WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
      "v1",
      new Microsoft.OpenApi.OpenApiInfo
      {
          Title = "Expense Tracker API",
          Version = "v1",
          Description = "HTTP API for sync and health checks. Pair with the Flutter app via AZURE_API_BASE_URL.",
      });
});

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

app.Run();
