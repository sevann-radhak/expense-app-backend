using Xunit;

namespace ExpenseTracker.Migrations.Tests;

public sealed class MigrationScriptEmbeddedTests
{
    [Fact]
    public void Book_schema_sql_is_embedded_in_migrations_assembly()
    {
        var asm = typeof(BookDatabaseMigrator).Assembly;
        var names = asm.GetManifestResourceNames();
        Assert.Contains(
            names,
            n => n.Contains("CreateBookSchema", StringComparison.OrdinalIgnoreCase)
                && n.EndsWith(".sql", StringComparison.OrdinalIgnoreCase));
    }
}
