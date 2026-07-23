using landerist_library.Application.Pages;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
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
}