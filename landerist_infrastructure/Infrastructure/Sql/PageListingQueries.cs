using landerist_library.Database;
using landerist_library.Pages;
using System.Data;
using PageTable = landerist_library.Pages.Pages;

namespace landerist_library.Infrastructure.Sql;

internal sealed class PageListingQueries
{
    private readonly IDatabase _database;

    public PageListingQueries(IDatabase database) => _database = database;

    public DataTable GetListingsWithHttpStatusCodeError() =>
        QueryPages(
            "WHERE " + PageTable.PAGES + ".[PageType] = 'Listing' " +
            "AND " + PageTable.PAGES + ".[HttpStatusCode] <> 200");

    public DataTable GetListingsWithParserInputHash() =>
        QueryPages(
            "WHERE " + PageTable.PAGES + ".[PageType] = 'Listing' " +
            "AND " + PageTable.PAGES + ".[ListingParserInputHash] IS NOT NULL");

    private DataTable QueryPages(string where) =>
        _database.QueryTable(
            PageSqlQueryBuilder.Select() + where,
            new Dictionary<string, object?>());
}
