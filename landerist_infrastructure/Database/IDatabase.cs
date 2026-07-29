using Microsoft.Data.SqlClient;
using System.Data;

namespace landerist_library.Database;

public interface IDatabase
{
    void SetTimeout(int timeOut);

    bool Query(string query, IDictionary<string, object?>? parameters = null);

    Task<bool> QueryAsync(
        string query,
        IDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    bool Query(
        string query,
        IDictionary<string, object?>? parameters,
        out Exception? exception);

    bool QueryBool(
        string query,
        IDictionary<string, object?>? parameters = null);

    Task<bool> QueryBoolAsync(
        string query,
        IDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);

    bool QueryExists(
        string querySelect1,
        IDictionary<string, object?>? parameters = null);

    int QueryInt(string query, IDictionary<string, object?>? parameters = null);

    string? QueryString(
        string query,
        IDictionary<string, object?>? parameters = null);

    DataTable QueryTable(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);

    Task<DataTable> QueryTableAsync(
        string query,
        IDictionary<string, object?>? parameters = null,
        CancellationToken cancellationToken = default);
    List<string> QueryListString(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);

    HashSet<string> QueryHashSet(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);

    Dictionary<string, object?> QueryDictionary(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);
}
