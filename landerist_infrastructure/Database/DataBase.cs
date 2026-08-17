using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace landerist_library.Database
{
    public class DataBase : IDatabase
    {
        private const int DefaultCommandTimeoutSeconds = 120;

        private readonly SqlCommandExecutor _executor;
        private readonly SqlQueryStack _queryStack = new();


        public DataBase(
            string connectionString,
            int commandTimeoutSeconds = DefaultCommandTimeoutSeconds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commandTimeoutSeconds);
            _executor = new SqlCommandExecutor(connectionString, commandTimeoutSeconds);
        }

        /// <summary>
        /// Sets the command timeout in seconds for subsequent operations.
        /// </summary>
        public void SetTimeout(int timeOut)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeOut);
            _executor.SetTimeout(timeOut);
        }

        public static List<SqlParameter> ParseParameters(Dictionary<string, object?> parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            return parameters
                .Select(item => SqlParameterBinder.Create(item.Key, item.Value))
                .ToList();
        }

        public bool Query(string query)
        {
            return _executor.Execute(
                operationName: nameof(Query),
                query,
                parameters: null,
                sqlParameters: null,
                command =>
                {
                    command.ExecuteNonQuery();
                    return true;
                },
                failureResult: false,
                out _);
        }

        public Task<bool> QueryAsync(
            string query,
            IDictionary<string, object?>? parameters = null,
            CancellationToken cancellationToken = default) =>
            _executor.ExecuteAsync(
                operationName: nameof(QueryAsync),
                query,
                parameters,
                async (command, token) =>
                {
                    await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                    return true;
                },
                cancellationToken);
        public bool Query(string query, string parameterName, object parameterValue)
        {
            return Query(query, new Dictionary<string, object?>
            {
                { parameterName, parameterValue }
            });
        }

        public bool Query(string query, IDictionary<string, object?>? parameters = null)
        {
            return _executor.Execute(
                operationName: nameof(Query),
                query,
                parameters,
                sqlParameters: null,
                command =>
                {
                    command.ExecuteNonQuery();
                    return true;
                },
                failureResult: false,
                out _);
        }

        public bool Query(
            string query,
            IDictionary<string, object?>? parameters,
            out Exception? exception)
        {
            return _executor.Execute(
                operationName: nameof(Query),
                query,
                parameters,
                sqlParameters: null,
                command =>
                {
                    command.ExecuteNonQuery();
                    return true;
                },
                failureResult: false,
                out exception,
                returnFailureResult: true);
        }

        public bool Query(string query, List<SqlParameter> sqlParameters)
        {
            ArgumentNullException.ThrowIfNull(sqlParameters);
            return Query(query, sqlParameters.ToArray());
        }

        public bool Query(string query, SqlParameter[] sqlParameters)
        {
            ArgumentNullException.ThrowIfNull(sqlParameters);
            return _executor.Execute(
                operationName: nameof(Query),
                query,
                parameters: null,
                sqlParameters,
                command =>
                {
                    command.ExecuteNonQuery();
                    return true;
                },
                failureResult: false,
                out _);
        }

        public bool QueryBool(string query, IDictionary<string, object?>? parameters = null)
        {
            return ExecuteScalar(
                nameof(QueryBool),
                query,
                parameters,
                defaultValue: false,
                value => Convert.ToBoolean(value, CultureInfo.InvariantCulture));
        }

        public Task<bool> QueryBoolAsync(
            string query,
            IDictionary<string, object?>? parameters = null,
            CancellationToken cancellationToken = default) =>
            _executor.ExecuteAsync(
                operationName: nameof(QueryBoolAsync),
                query,
                parameters,
                async (command, token) =>
                {
                    object? value = await command
                        .ExecuteScalarAsync(token)
                        .ConfigureAwait(false);
                    return value is not null &&
                        value != DBNull.Value &&
                        Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                },
                cancellationToken);
        public bool QueryExists(
            string querySelect1,
            IDictionary<string, object?>? parameters = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(querySelect1);
            string query =
                "SELECT CASE " +
                $"WHEN EXISTS ({querySelect1}) THEN CAST(1 AS BIT) " +
                "ELSE CAST(0 AS BIT) " +
                "END";

            return QueryBool(query, parameters);
        }

        public int QueryInt(string query, IDictionary<string, object?>? parameters = null)
        {
            return ExecuteScalar(
                nameof(QueryInt),
                query,
                parameters,
                defaultValue: 0,
                value => Convert.ToInt32(value, CultureInfo.InvariantCulture));
        }

        public long QueryLong(string query, IDictionary<string, object?>? parameters = null)
        {
            return ExecuteScalar(
                nameof(QueryLong),
                query,
                parameters,
                defaultValue: 0L,
                value => Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }

        public string? QueryString(
            string query,
            IDictionary<string, object?>? parameters = null)
        {
            return ExecuteScalar<string?>(
                nameof(QueryString),
                query,
                parameters,
                defaultValue: null,
                value => Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public Guid QueryGuid(string query, IDictionary<string, object?>? parameters = null)
        {
            return ExecuteScalar(
                nameof(QueryGuid),
                query,
                parameters,
                defaultValue: Guid.Empty,
                value => value is Guid guid ? guid : Guid.Parse(value.ToString()!));
        }

        public DataTable QueryTable(string query, string parameterName, object? parameterValue)
        {
            return QueryTable(query, new Dictionary<string, object?>
            {
                { parameterName, parameterValue }
            });
        }

        public DataTable QueryTable(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            return _executor.Execute(
                operationName: nameof(QueryTable),
                query,
                parameters,
                sqlParameters,
                command =>
                {
                    DataTable dataTable = new();
                    using SqlDataAdapter adapter = new(command);
                    adapter.Fill(dataTable);
                    return dataTable;
                },
                failureResult: new DataTable(),
                out _);
        }

        public Task<DataTable> QueryTableAsync(
            string query,
            IDictionary<string, object?>? parameters = null,
            CancellationToken cancellationToken = default) =>
            _executor.ExecuteAsync(
                operationName: nameof(QueryTableAsync),
                query,
                parameters,
                SqlDataReaderMapper.ReadTableAsync,
                cancellationToken);
        public DataSet QueryDataSet(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            return _executor.Execute(
                operationName: nameof(QueryDataSet),
                query,
                parameters,
                sqlParameters,
                command =>
                {
                    DataSet dataSet = new();
                    using SqlDataAdapter adapter = new(command);
                    adapter.Fill(dataSet);
                    return dataSet;
                },
                failureResult: new DataSet(),
                out _);
        }

        public List<string> QueryListString(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            return ExecuteReader(
                nameof(QueryListString),
                query,
                parameters,
                sqlParameters,
                reader => reader.GetString(0));
        }

        public List<int> QueryListInt(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            return ExecuteReader(
                nameof(QueryListInt),
                query,
                parameters,
                sqlParameters,
                reader => reader.GetInt32(0));
        }

        public HashSet<string> QueryHashSet(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            var values = ExecuteReader(
                nameof(QueryHashSet),
                query,
                parameters,
                sqlParameters,
                reader => reader.GetString(0));

            return values.ToHashSet(StringComparer.Ordinal);
        }

        public Dictionary<string, object?> QueryDictionary(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            var pairs = ExecuteReader(
                nameof(QueryDictionary),
                query,
                parameters,
                sqlParameters,
                reader => new KeyValuePair<string, object?>(
                    reader.IsDBNull(0) ? "null" : reader.GetString(0),
                    reader.IsDBNull(1) ? null : reader.GetValue(1)));

            Dictionary<string, object?> dictionary = [];
            foreach (var pair in pairs)
            {
                dictionary[pair.Key] = pair.Value;
            }

            return dictionary;
        }

        public bool StackQuery(string query, int maxQueries = 1000)
        {
            string? queryToFlush = _queryStack.Append(query, maxQueries);
            return queryToFlush is null || Query(queryToFlush);
        }

        public bool StackFlush()
        {
            string? query = _queryStack.Take();
            return query is null || Query(query);
        }

        private T ExecuteScalar<T>(
            string operationName,
            string query,
            IDictionary<string, object?>? parameters,
            T defaultValue,
            Func<object, T> convert)
        {
            return _executor.Execute(
                operationName,
                query,
                parameters,
                sqlParameters: null,
                command =>
                {
                    object? value = command.ExecuteScalar();
                    return value is null or DBNull ? defaultValue : convert(value);
                },
                failureResult: defaultValue,
                out _);
        }

        private List<T> ExecuteReader<T>(
            string operationName,
            string query,
            IDictionary<string, object?>? parameters,
            SqlParameter[]? sqlParameters,
            Func<SqlDataReader, T> map)
        {
            return _executor.Execute(
                operationName,
                query,
                parameters,
                sqlParameters,
                command => SqlDataReaderMapper.ReadList(command, map),
                failureResult: [],
                out _);
        }
    }
}
