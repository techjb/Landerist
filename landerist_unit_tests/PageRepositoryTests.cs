using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using Microsoft.Data.SqlClient;
using System.Data;

namespace landerist_unit_tests;

public sealed class PageRepositoryTests
{
    [Fact]
    public void GetDataRow_UsesInjectedDatabaseAndReturnsFirstRow()
    {
        RecordingDatabase database = new();
        database.TableResult.Columns.Add("UriHash", typeof(string));
        database.TableResult.Rows.Add("expected-hash");
        PageRepository repository = new(database);

        DataRow? result = repository.GetDataRow("expected-hash");

        Assert.NotNull(result);
        Assert.Equal("expected-hash", result["UriHash"]);
        Assert.Contains("WHERE [UriHash] = @UriHash", database.LastQuery);
        Assert.Equal("expected-hash", database.LastParameters!["UriHash"]);
    }

    [Fact]
    public void Insert_DelegatesParametersAndResult()
    {
        RecordingDatabase database = new() { QueryResult = true };
        PageRepository repository = new(database);
        Dictionary<string, object?> parameters = new()
        {
            ["UriHash"] = "expected-hash"
        };

        bool result = repository.Insert(parameters);

        Assert.True(result);
        Assert.Same(parameters, database.LastParameters);
        Assert.Contains("INSERT INTO", database.LastQuery);
    }

    [Fact]
    public void Update_PropagatesDatabaseException()
    {
        InvalidOperationException expectedException = new("Expected failure");
        RecordingDatabase database = new()
        {
            QueryResult = false,
            QueryException = expectedException
        };
        PageRepository repository = new(database);

        bool result = repository.Update(new Dictionary<string, object?>(), out Exception? exception);

        Assert.False(result);
        Assert.Same(expectedException, exception);
        Assert.Contains("UPDATE", database.LastQuery);
    }

    [Fact]
    public void CountPages_UsesInjectedDatabase()
    {
        RecordingDatabase database = new() { QueryIntResult = 42 };
        PageQueryRepository repository = new(database);

        int result = repository.CountPages();

        Assert.Equal(42, result);
        Assert.Contains("COUNT(*)", database.LastQuery);
    }

    private sealed class RecordingDatabase : IDatabase
    {
        public string LastQuery { get; private set; } = string.Empty;
        public IDictionary<string, object?>? LastParameters { get; private set; }
        public bool QueryResult { get; init; }
        public Exception? QueryException { get; init; }
        public int QueryIntResult { get; init; }
        public DataTable TableResult { get; } = new();

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
            return [];
        }

        private void Record(string query, IDictionary<string, object?>? parameters)
        {
            LastQuery = query;
            LastParameters = parameters;
        }
    }
}
