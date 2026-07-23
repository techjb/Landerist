using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using Microsoft.Data.SqlClient;

namespace landerist_unit_tests;

public sealed class DatabaseCompositionTests
{
    [Fact]
    public void Options_BuildExpectedConnectionString()
    {
        SqlDatabaseOptions options = new(
            "sql.example.test,1433",
            "landerist-user",
            "secret",
            "landerist-db",
            encrypt: true,
            trustServerCertificate: false,
            connectionTimeoutSeconds: 15,
            commandTimeoutSeconds: 45);

        SqlConnectionStringBuilder connection = new(options.ConnectionString);

        Assert.Equal("sql.example.test,1433", connection.DataSource);
        Assert.Equal("landerist-user", connection.UserID);
        Assert.Equal("secret", connection.Password);
        Assert.Equal("landerist-db", connection.InitialCatalog);
        Assert.True(connection.Encrypt);
        Assert.False(connection.TrustServerCertificate);
        Assert.Equal(15, connection.ConnectTimeout);
        Assert.Equal(45, options.CommandTimeoutSeconds);
    }

    [Fact]
    public void Options_RejectInvalidTimeouts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SqlDatabaseOptions("server", "user", "password", "database", false, false, 0, 120));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SqlDatabaseOptions("server", "user", "password", "database", false, false, 30, 0));
    }

    [Fact]
    public void Factory_CreatesIndependentDatabaseExecutors()
    {
        SqlDatabaseFactory factory = new(
            new SqlDatabaseOptions(
                "server",
                "user",
                "password",
                "database",
                encrypt: false,
                trustServerCertificate: true));

        IDatabase first = factory.Create();
        IDatabase second = factory.Create();

        Assert.IsType<DataBase>(first);
        Assert.IsType<DataBase>(second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void LegacyBridge_UsesConfiguredFactory()
    {
        RecordingDatabase database = new();
        RecordingDatabaseFactory factory = new(database);

        LegacyDatabase.Configure(factory);
        IDatabase result = LegacyDatabase.Create();

        Assert.Same(database, result);
        Assert.Equal(1, factory.CreateCalls);
    }

    private sealed class RecordingDatabaseFactory(IDatabase database) : IDatabaseFactory
    {
        public int CreateCalls { get; private set; }

        public IDatabase Create()
        {
            CreateCalls++;
            return database;
        }
    }
}
