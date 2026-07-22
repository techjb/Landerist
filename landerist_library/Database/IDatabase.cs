using Microsoft.Data.SqlClient;
using System.Data;

namespace landerist_library.Database;

public interface IDatabase
{
    bool Query(string query, IDictionary<string, object?>? parameters = null);

    bool Query(
        string query,
        IDictionary<string, object?>? parameters,
        out Exception? exception);

    bool QueryExists(
        string querySelect1,
        IDictionary<string, object?>? parameters = null);

    int QueryInt(string query, IDictionary<string, object?>? parameters = null);

    DataTable QueryTable(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);

    List<string> QueryListString(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);

    Dictionary<string, object?> QueryDictionary(
        string query,
        IDictionary<string, object?>? parameters = null,
        SqlParameter[]? sqlParameters = null);
}
