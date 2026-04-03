namespace ExpenseTracker.Infrastructure.Data;

public static class EfDesignTimeDefaults
{
    public const string ConnectionEnvironmentVariable = "ConnectionStrings__DefaultConnection";

    public const string FallbackConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=ExpenseTrackerEfDesign;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
}
