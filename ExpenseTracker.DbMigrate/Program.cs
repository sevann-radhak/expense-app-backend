using ExpenseTracker.Migrations;

var connectionString =
    Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("EXPENSE_TRACKER_CONNECTION_STRING")
    ?? (args.Length > 0 ? args[0] : null);

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine(
        "Set ConnectionStrings__DefaultConnection or EXPENSE_TRACKER_CONNECTION_STRING, or pass the connection string as the first argument.");
    return 1;
}

var createDb = string.Equals(
    Environment.GetEnvironmentVariable("EXPENSE_TRACKER_CREATE_DATABASE"),
    "true",
    StringComparison.OrdinalIgnoreCase);

var result = BookDatabaseMigrator.Apply(connectionString, createDatabaseIfMissing: createDb);

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error?.Message ?? "Migration failed.");
    return 2;
}

Console.WriteLine("Database migrations applied successfully.");
return 0;
