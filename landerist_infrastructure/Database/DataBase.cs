using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace landerist_library.Database
{
    public class DataBase : IDatabase
    {
        private const int DefaultCommandTimeoutSeconds = 120;

        private readonly StringBuilder _stackBuilder = new();
        private readonly object _stackLock = new();
        private readonly string _connectionString;
        private int _stackCounter;
        private int _commandTimeout;


        public DataBase(
            string connectionString,
            int commandTimeoutSeconds = DefaultCommandTimeoutSeconds)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(commandTimeoutSeconds);
            _connectionString = connectionString;
            _commandTimeout = commandTimeoutSeconds;
        }

        /// <summary>
        /// Sets the command timeout in seconds for subsequent operations.
        /// </summary>
        public void SetTimeout(int timeOut)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(timeOut);
            _commandTimeout = timeOut;
        }

        public static List<SqlParameter> ParseParameters(Dictionary<string, object?> parameters)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            return parameters
                .Select(item => CreateParameter(item.Key, item.Value))
                .ToList();
        }

        public bool Query(string query)
        {
            return Execute(
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
            ExecuteAsync(
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
            return Execute(
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
            return Execute(
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
            return Execute(
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
            return Execute(
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

        public DataSet QueryDataSet(
            string query,
            IDictionary<string, object?>? parameters = null,
            SqlParameter[]? sqlParameters = null)
        {
            return Execute(
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
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxQueries);

            string? queryToFlush = null;
            lock (_stackLock)
            {
                _stackBuilder.Append(query).Append(' ');
                _stackCounter++;

                if (_stackCounter >= maxQueries)
                {
                    queryToFlush = TakeStackQuery();
                }
            }

            return queryToFlush is null || Query(queryToFlush);
        }

        public bool StackFlush()
        {
            string? query;
            lock (_stackLock)
            {
                query = TakeStackQuery();
            }

            return query is null || Query(query);
        }

        private string? TakeStackQuery()
        {
            if (_stackCounter == 0)
            {
                return null;
            }

            string query = _stackBuilder.ToString();
            _stackBuilder.Clear();
            _stackCounter = 0;
            return query;
        }

        private T ExecuteScalar<T>(
            string operationName,
            string query,
            IDictionary<string, object?>? parameters,
            T defaultValue,
            Func<object, T> convert)
        {
            return Execute(
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
            return Execute(
                operationName,
                query,
                parameters,
                sqlParameters,
                command =>
                {
                    List<T> values = [];
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        values.Add(map(reader));
                    }

                    return values;
                },
                failureResult: [],
                out _);
        }

        private T Execute<T>(
            string operationName,
            string query,
            IDictionary<string, object?>? parameters,
            SqlParameter[]? sqlParameters,
            Func<SqlCommand, T> operation,
            T failureResult,
            out Exception? exception,
            bool returnFailureResult = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(operation);

            exception = null;
            try
            {
                using SqlConnection connection = new(_connectionString);
                using SqlCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = _commandTimeout;
                AddParameters(command, parameters, sqlParameters);

                connection.Open();
                return operation(command);
            }
            catch (Exception ex)
            {
                exception = ex;
                Trace.TraceError("Database operation {0} failed: {1}", operationName, ex);
                if (returnFailureResult)
                {
                    return failureResult;
                }

                throw new DatabaseOperationException(operationName, ex);
            }
        }

        private async Task<T> ExecuteAsync<T>(
            string operationName,
            string query,
            IDictionary<string, object?>? parameters,
            Func<SqlCommand, CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(operation);
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await using SqlConnection connection = new(_connectionString);
                await using SqlCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = _commandTimeout;
                AddParameters(command, parameters, sqlParameters: null);

                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                return await operation(command, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Database operation {0} failed: {1}", operationName, ex);
                throw new DatabaseOperationException(operationName, ex);
            }
        }
        private static void AddParameters(
            SqlCommand command,
            IDictionary<string, object?>? parameters,
            SqlParameter[]? sqlParameters)
        {
            if (parameters is not null)
            {
                foreach (var item in parameters)
                {
                    command.Parameters.Add(CreateParameter(item.Key, item.Value));
                }
            }

            if (sqlParameters is not null)
            {
                foreach (SqlParameter parameter in sqlParameters)
                {
                    command.Parameters.Add((SqlParameter)((ICloneable)parameter).Clone());
                }
            }
        }

        private static SqlParameter CreateParameter(string name, object? value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            string parameterName = name.StartsWith('@') ? name : "@" + name;
            return new SqlParameter(parameterName, value ?? DBNull.Value);
        }
    }
}
