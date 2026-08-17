using Microsoft.Data.SqlClient;
using System.Diagnostics;

namespace landerist_library.Database;

internal sealed class SqlCommandExecutor
{
    private readonly string _connectionString;
    private int _commandTimeout;

    public SqlCommandExecutor(string connectionString, int commandTimeout)
    {
        _connectionString = connectionString;
        _commandTimeout = commandTimeout;
    }

    public void SetTimeout(int commandTimeout) => _commandTimeout = commandTimeout;

    public T Execute<T>(
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
            using SqlCommand command = CreateCommand(connection, query, parameters, sqlParameters);
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

    public async Task<T> ExecuteAsync<T>(
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
            await using SqlCommand command = CreateCommand(
                connection,
                query,
                parameters,
                sqlParameters: null);
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

    private SqlCommand CreateCommand(
        SqlConnection connection,
        string query,
        IDictionary<string, object?>? parameters,
        SqlParameter[]? sqlParameters)
    {
        SqlCommand command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = _commandTimeout;
        SqlParameterBinder.AddTo(command, parameters, sqlParameters);
        return command;
    }
}
