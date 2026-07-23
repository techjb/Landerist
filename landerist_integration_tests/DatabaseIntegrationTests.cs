using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;

namespace landerist_integration_tests;

public sealed class DatabaseIntegrationTests
{
    private static readonly Lazy<bool> TestConfiguration = new(() =>
    {
        Config.SetToTest();
        return true;
    });

    [Fact]
    public void ConfiguredDatabase_CanBeReached()
    {
        EnsureTestConfiguration();

        bool connected = CreateDatabase().Query("SELECT 1", null, out Exception? exception);

        Assert.True(connected, exception?.ToString() ?? "Database connection failed.");
    }

    [Fact]
    public void ConfiguredDatabase_SupportsIsolatedCrudOperations()
    {
        EnsureTestConfiguration();

        IDatabase database = CreateDatabase();
        var parameters = new Dictionary<string, object?>
        {
            ["Id"] = Guid.NewGuid(),
            ["InitialValue"] = "initial",
            ["UpdatedValue"] = "updated"
        };

        const string query = """
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            CREATE TABLE #LanderistDatabaseIntegrationTest
            (
                [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                [Value] NVARCHAR(50) NOT NULL
            );

            INSERT INTO #LanderistDatabaseIntegrationTest ([Id], [Value])
            VALUES (@Id, @InitialValue);

            IF @@ROWCOUNT <> 1
                THROW 51000, 'The integration test insert failed.', 1;

            IF NOT EXISTS (
                SELECT 1
                FROM #LanderistDatabaseIntegrationTest
                WHERE [Id] = @Id AND [Value] = @InitialValue)
                THROW 51001, 'The integration test read failed.', 1;

            UPDATE #LanderistDatabaseIntegrationTest
            SET [Value] = @UpdatedValue
            WHERE [Id] = @Id;

            IF @@ROWCOUNT <> 1
                THROW 51002, 'The integration test update failed.', 1;

            IF NOT EXISTS (
                SELECT 1
                FROM #LanderistDatabaseIntegrationTest
                WHERE [Id] = @Id AND [Value] = @UpdatedValue)
                THROW 51003, 'The integration test update verification failed.', 1;

            DELETE FROM #LanderistDatabaseIntegrationTest
            WHERE [Id] = @Id;

            IF @@ROWCOUNT <> 1
                THROW 51004, 'The integration test delete failed.', 1;

            IF EXISTS (
                SELECT 1
                FROM #LanderistDatabaseIntegrationTest
                WHERE [Id] = @Id)
                THROW 51005, 'The integration test delete verification failed.', 1;
            """;

        bool succeeded = database.Query(query, parameters, out Exception? exception);

        Assert.True(succeeded, exception?.ToString() ?? "Database CRUD test failed.");
    }

    private static IDatabase CreateDatabase()
    {
        EnsureTestConfiguration();
        SqlDatabaseFactory factory = new(
            new SqlDatabaseOptions(
                Config.DATASOURCE
                    ?? throw new InvalidOperationException("Database data source is not configured."),
                Config.DATABASE_USER,
                Config.DATABASE_PW,
                Config.DATABASE_NAME,
                Config.DATABASE_ENCRYPT,
                Config.DATABASE_TRUST_SERVER_CERTIFICATE));
        return factory.Create();
    }

    private static void EnsureTestConfiguration()
    {
        _ = TestConfiguration.Value;
    }
}

