using landerist_library.Database;
using Microsoft.Data.SqlClient;
using System.Data;

namespace landerist_unit_tests;

internal sealed class RecordingDatabase : IDatabase
{
    public string LastQuery { get; private set; } = string.Empty;
    public IDictionary<string, object?>? LastParameters { get; private set; }
    public bool QueryResult { get; init; }
    public Exception? QueryException { get; init; }
    public int QueryIntResult { get; init; }
    public DataTable TableResult { get; } = new();
    public List<string> ListStringResult { get; } = [];
    public bool QueryExistsResult { get; init; }
    public Dictionary<string, object?> DictionaryResult { get; } = [];
    public HashSet<string> HashSetResult { get; } = [];

    public bool Query(string query, IDictionary<string, object?>? parameters = null)
    {
        Record(query, parameters);
        return QueryResult;
    }

    public bool Query(
        string query,
        IDictionary<string, object?>? parameters,
        out Exception? exception)
    {
        Record(query, parameters);
        exception = QueryException;
        return QueryResult;
    }

    public bool QueryExists(
        string query,
        IDictionary<string, object?>? parameters = null)
    {
        Record(query, parameters);
        return QueryExistsResult;
    }

    public int QueryInt(string query, IDictionary<string, object?>? parameters = null)
    {
        Record(query, parameters);
        return QueryIntResult;
    }

    public DataTable QueryTable(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null)
    {
        Record(query, parameters);
        return TableResult;
    }

    public List<string> QueryListString(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null)
    {
        Record(query, parameters);
        return ListStringResult;
    }

    public HashSet<string> QueryHashSet(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null)
    {
        Record(query, parameters);
        return HashSetResult;
    }

    public Dictionary<string, object?> QueryDictionary(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null)
    {
        Record(query, parameters);
        return DictionaryResult;
    }

    private void Record(string query, IDictionary<string, object?>? parameters)
    {
        LastQuery = query;
        LastParameters = parameters;
    }
}
