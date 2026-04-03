using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExpenseTrackerSqlServer(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _ = services.AddDbContext<ExpenseTrackerDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(ExpenseTrackerDbContext).Assembly.FullName)));
        _ = services.AddScoped<DevBookDataService>();
        _ = services.AddScoped<BookSyncService>();
        return services;
    }
}
