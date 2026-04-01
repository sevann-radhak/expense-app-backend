using Microsoft.Data.SqlClient;
using Xunit;

namespace ExpenseTracker.Migrations.Tests;

public sealed class TenantIsolationTests
{
    /// <summary>
    /// Set EXPENSE_TRACKER_TEST_SQL to a SQL Server / Azure SQL connection string with DDL permission.
    /// Skipped in CI when unset. Validates composite (user_id, id) scoping after migrations.
    /// </summary>
    [Fact]
    public async Task Categories_from_user_a_are_not_visible_when_querying_as_user_b()
    {
        var cs = Environment.GetEnvironmentVariable("EXPENSE_TRACKER_TEST_SQL");
        if (string.IsNullOrWhiteSpace(cs))
        {
            return;
        }

        var userA = "test-user-a-" + Guid.NewGuid().ToString("N");
        var userB = "test-user-b-" + Guid.NewGuid().ToString("N");
        var catA = "cat-a-" + Guid.NewGuid().ToString("N");
        var catB = "cat-b-" + Guid.NewGuid().ToString("N");

        var result = BookDatabaseMigrator.Apply(cs, createDatabaseIfMissing: false);
        Assert.True(result.Successful, result.Error?.ToString());

        await using (var conn = new SqlConnection(cs))
        {
            await conn.OpenAsync();

            await using (var cleanup = conn.CreateCommand())
            {
                cleanup.CommandText = """
                    DELETE FROM dbo.categories WHERE user_id IN (@a, @b);
                    """;
                _ = cleanup.Parameters.AddWithValue("@a", userA);
                _ = cleanup.Parameters.AddWithValue("@b", userB);
                _ = await cleanup.ExecuteNonQueryAsync();
            }

            await using (var insert = conn.CreateCommand())
            {
                insert.CommandText = """
                    INSERT INTO dbo.categories (user_id, id, name, sort_order, is_active)
                    VALUES (@ua, @ida, N'Alpha', 0, 1), (@ub, @idb, N'Beta', 0, 1);
                    """;
                _ = insert.Parameters.AddWithValue("@ua", userA);
                _ = insert.Parameters.AddWithValue("@ida", catA);
                _ = insert.Parameters.AddWithValue("@ub", userB);
                _ = insert.Parameters.AddWithValue("@idb", catB);
                _ = await insert.ExecuteNonQueryAsync();
            }

            await using (var select = conn.CreateCommand())
            {
                select.CommandText = """
                    SELECT COUNT(*) FROM dbo.categories WHERE user_id = @u;
                    """;
                _ = select.Parameters.AddWithValue("@u", userA);
                var countA = (int)(await select.ExecuteScalarAsync() ?? 0);
                Assert.Equal(1, countA);

                select.Parameters.Clear();
                _ = select.Parameters.AddWithValue("@u", userB);
                var countB = (int)(await select.ExecuteScalarAsync() ?? 0);
                Assert.Equal(1, countB);
            }

            await using (var selectNames = conn.CreateCommand())
            {
                selectNames.CommandText = """
                    SELECT name FROM dbo.categories WHERE user_id = @u AND id = @id;
                    """;
                _ = selectNames.Parameters.AddWithValue("@u", userA);
                _ = selectNames.Parameters.AddWithValue("@id", catB);
                var nameWrong = await selectNames.ExecuteScalarAsync();
                Assert.Null(nameWrong);
            }

            await using (var cleanup = conn.CreateCommand())
            {
                cleanup.CommandText = """
                    DELETE FROM dbo.categories WHERE user_id IN (@a, @b);
                    """;
                _ = cleanup.Parameters.AddWithValue("@a", userA);
                _ = cleanup.Parameters.AddWithValue("@b", userB);
                _ = await cleanup.ExecuteNonQueryAsync();
            }
        }
    }
}
