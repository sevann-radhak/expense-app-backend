namespace ExpenseTracker.Api.Hosting;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAppSettingsLocal(this WebApplicationBuilder builder)
    {
        _ = builder.Configuration.AddJsonFile(
            Path.Combine(builder.Environment.ContentRootPath, "appsettings.local.json"),
            optional: true,
            reloadOnChange: true);
        return builder;
    }
}
