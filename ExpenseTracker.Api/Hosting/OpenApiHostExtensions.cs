using ExpenseTracker.Api.Configuration;
using Microsoft.OpenApi;

namespace ExpenseTracker.Api.Hosting;

public static class OpenApiHostExtensions
{
    public static IServiceCollection AddExpenseTrackerOpenApi(this IServiceCollection services, OpenApiOptions o)
    {
        _ = services.AddEndpointsApiExplorer();
        _ = services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                o.DocumentName,
                new OpenApiInfo
                {
                    Title = o.Title,
                    Version = o.Version,
                    Description = o.Description,
                });
            options.AddSecurityDefinition(
                o.SecuritySchemeId,
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = o.SecuritySchemeDescription,
                });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(o.SecuritySchemeId, document)] = [],
            });
        });
        return services;
    }

    public static WebApplication UseExpenseTrackerSwaggerUi(this WebApplication app, OpenApiOptions o)
    {
        _ = app.UseSwagger();
        _ = app.UseSwaggerUI(options => { options.SwaggerEndpoint(o.SwaggerJsonPath, o.SwaggerUiDocumentTitle); });
        return app;
    }
}
