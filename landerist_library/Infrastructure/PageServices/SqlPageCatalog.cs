using landerist_library.Application.Pages;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.PageServices;

public sealed class SqlPageCatalog : IPageCatalog
{
    private readonly PageQueryRepository _repository;

    public SqlPageCatalog(PageQueryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public Page? GetByHash(string uriHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uriHash);
        DataTable rows = _repository.GetPageByUriHash(uriHash);
        if (rows.Rows.Count != 1)
        {
            return null;
        }

        DataRow row = rows.Rows[0];
        var website = WebsiteDataMapper.Map(row);
        return PageDataMapper.Map(row, website);
    }

    public IReadOnlyList<Page> GetByWebsite(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        DataTable rows = _repository.GetPagesByHost(website.Host);
        List<Page> pages = [];
        foreach (DataRow row in rows.Rows)
        {
            pages.Add(PageDataMapper.Map(row, website));
        }
        return pages;
    }
}
