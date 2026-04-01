using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseTracker.Infrastructure.Data;

/// <summary>Used by <c>dotnet ef migrations</c> when the startup project does not supply a context at design time.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ExpenseTrackerDbContext>
{
    public ExpenseTrackerDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<ExpenseTrackerDbContext> options = new DbContextOptionsBuilder<ExpenseTrackerDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\MSSQLLocalDB;Database=ExpenseTrackerEfDesign;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true")
            .Options;
        return new ExpenseTrackerDbContext(options);
    }
}
