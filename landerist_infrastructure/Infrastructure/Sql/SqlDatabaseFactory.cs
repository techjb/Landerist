using landerist_library.Database;

namespace landerist_library.Infrastructure.Sql;

public sealed class SqlDatabaseFactory : IDatabaseFactory
{
    private readonly SqlDatabaseOptions _options;

    public SqlDatabaseFactory(SqlDatabaseOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    public IDatabase Create() =>
        new DataBase(
            _options.ConnectionString,
            _options.CommandTimeoutSeconds);
}
