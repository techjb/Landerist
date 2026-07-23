using Microsoft.Data.SqlClient;

namespace landerist_library.Infrastructure.Sql;

public sealed class SqlDatabaseOptions
{
    public SqlDatabaseOptions(
        string dataSource,
        string userId,
        string password,
        string databaseName,
        bool encrypt,
        bool trustServerCertificate,
        int connectionTimeoutSeconds = 30,
        int commandTimeoutSeconds = 120)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(connectionTimeoutSeconds);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commandTimeoutSeconds);

        ConnectionString = new SqlConnectionStringBuilder
        {
            DataSource = dataSource,
            UserID = userId,
            Password = password,
            InitialCatalog = databaseName,
            ConnectTimeout = connectionTimeoutSeconds,
            Encrypt = encrypt,
            TrustServerCertificate = trustServerCertificate
        }.ConnectionString;
        CommandTimeoutSeconds = commandTimeoutSeconds;
    }

    public string ConnectionString { get; }

    public int CommandTimeoutSeconds { get; }
}
