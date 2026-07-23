using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlPageSelectionRepository : IPageSelectionRepository
{
    private readonly PageQueryRepository _queries;
    private readonly PageMaintenanceRepository _maintenance;
    private readonly string _machineName;

    public SqlPageSelectionRepository(IDatabase database, string machineName)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
        _queries = new PageQueryRepository(database);
        _maintenance = new PageMaintenanceRepository(database);
        _machineName = machineName;
    }

    public void CleanLockedPages() => _maintenance.CleanLockedBy(_machineName);

    public IReadOnlyList<Page> GetScrapePages(int maximumCount)
    {
        DataTable rows = _queries.GetScrapePages(maximumCount);
        List<Page> pages = new(rows.Rows.Count);
        foreach (DataRow row in rows.Rows)
        {
            var website = WebsiteDataMapper.Map(row);
            pages.Add(PageDataMapper.Map(row, website));
        }

        return pages;
    }
}
