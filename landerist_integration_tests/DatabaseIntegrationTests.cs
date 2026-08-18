using landerist_library.Database;
using landerist_library.Infrastructure.Sql;

namespace landerist_integration_tests;

public sealed class DatabaseIntegrationTests(SqlServerFixture fixture)
    : IClassFixture<SqlServerFixture>
{
    [SqlIntegrationFact]
    public void ConfiguredDatabase_CanBeReached()
    {
        bool connected = fixture.CreateDatabase()
            .Query("SELECT 1", null, out Exception? exception);

        Assert.True(connected, exception?.ToString() ?? "Database connection failed.");
    }

    [SqlIntegrationFact]
    public void ConfiguredDatabase_SupportsIsolatedCrudOperations()
    {
        IDatabase database = fixture.CreateDatabase();
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

            IF NOT EXISTS (
                SELECT 1 FROM #LanderistDatabaseIntegrationTest
                WHERE [Id] = @Id AND [Value] = @InitialValue)
                THROW 51001, 'The integration test insert/read failed.', 1;

            UPDATE #LanderistDatabaseIntegrationTest
            SET [Value] = @UpdatedValue
            WHERE [Id] = @Id;

            IF NOT EXISTS (
                SELECT 1 FROM #LanderistDatabaseIntegrationTest
                WHERE [Id] = @Id AND [Value] = @UpdatedValue)
                THROW 51002, 'The integration test update failed.', 1;

            DELETE FROM #LanderistDatabaseIntegrationTest WHERE [Id] = @Id;

            IF EXISTS (
                SELECT 1 FROM #LanderistDatabaseIntegrationTest WHERE [Id] = @Id)
                THROW 51003, 'The integration test delete failed.', 1;
            """;

        bool succeeded = database.Query(query, parameters, out Exception? exception);

        Assert.True(succeeded, exception?.ToString() ?? "Database CRUD test failed.");
    }

    [SqlIntegrationFact]
    public void Factory_CreatesIndependentUsableExecutors()
    {
        IDatabase first = fixture.CreateDatabase();
        IDatabase second = fixture.CreateDatabase();

        Assert.True(first.Query("SELECT 1"));
        Assert.True(second.Query("SELECT 1"));
        Assert.NotSame(first, second);
    }
}

public sealed class SqlServerFixture
{
    private readonly SqlDatabaseFactory? _factory;

    public SqlServerFixture()
    {
        if (!SqlIntegrationEnvironment.TryReadOptions(out SqlDatabaseOptions? options))
        {
            return;
        }

        _factory = new SqlDatabaseFactory(options!);
        WaitUntilAvailable();
    }

    public IDatabase CreateDatabase()
    {
        if (_factory is null)
        {
            throw new InvalidOperationException(
                "SQL integration tests require LANDERIST_TEST_SQL_* environment variables. " +
                "See landerist_integration_tests/README.md.");
        }

        return _factory.Create();
    }

    private void WaitUntilAvailable()
    {
        Exception? lastException = null;
        for (int attempt = 1; attempt <= 30; attempt++)
        {
            if (CreateDatabase().Query("SELECT 1", null, out lastException))
            {
                return;
            }
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        throw new InvalidOperationException(
            "SQL Server did not become available within 60 seconds.",
            lastException);
    }
}

internal static class SqlIntegrationEnvironment
{
    private static readonly string[] RequiredNames =
    [
        "LANDERIST_TEST_SQL_DATASOURCE",
        "LANDERIST_TEST_SQL_USER",
        "LANDERIST_TEST_SQL_PASSWORD",
        "LANDERIST_TEST_SQL_DATABASE"
    ];

    internal static bool HasAnyConfiguration() => RequiredNames.Any(name =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));

    internal static bool TryReadOptions(out SqlDatabaseOptions? options)
    {
        if (!HasAnyConfiguration())
        {
            options = null;
            return false;
        }

        options = new SqlDatabaseOptions(
        ReadRequired("LANDERIST_TEST_SQL_DATASOURCE"),
        ReadRequired("LANDERIST_TEST_SQL_USER"),
        ReadRequired("LANDERIST_TEST_SQL_PASSWORD"),
        ReadRequired("LANDERIST_TEST_SQL_DATABASE"),
        encrypt: ReadBoolean("LANDERIST_TEST_SQL_ENCRYPT", false),
        trustServerCertificate: ReadBoolean(
            "LANDERIST_TEST_SQL_TRUST_SERVER_CERTIFICATE",
            true),
        connectionTimeoutSeconds: 2,
        commandTimeoutSeconds: 30);
        return true;
    }

    private static string ReadRequired(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"Environment variable '{name}' is required to run integration tests.");

    private static bool ReadBoolean(string name, bool defaultValue) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
            ? bool.Parse(value)
            : defaultValue;
}

public sealed class SqlIntegrationFactAttribute : FactAttribute
{
    public SqlIntegrationFactAttribute()
    {
        if (!SqlIntegrationEnvironment.HasAnyConfiguration())
        {
            Skip =
                "Requires LANDERIST_TEST_SQL_* environment variables; " +
                "see landerist_integration_tests/README.md.";
        }
    }
}
