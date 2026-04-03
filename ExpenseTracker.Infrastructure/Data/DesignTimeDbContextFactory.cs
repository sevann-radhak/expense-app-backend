using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseTracker.Infrastructure.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExpenseTrackerDbContext>
{
    public ExpenseTrackerDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable(EfDesignTimeDefaults.ConnectionEnvironmentVariable)
            ?? EfDesignTimeDefaults.FallbackConnectionString;
        DbContextOptions<ExpenseTrackerDbContext> options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ExpenseTrackerDbContext(options);
    }
}
