using landerist_library.Application.Websites;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.WebsiteServices;

public sealed class SqlWebsiteCatalog : IWebsiteCatalog
{
    private readonly WebsiteQueryRepository _repository;

    public SqlWebsiteCatalog(WebsiteQueryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public IReadOnlyList<Website> GetAll() => Map(_repository.GetAll());

    public IReadOnlySet<string> GetHosts() => _repository.GetHosts();

    public Website Get(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        DataTable rows = _repository.GetWebsite(host);
        if (rows.Rows.Count == 0)
        {
            throw new KeyNotFoundException("Website not found for host: " + host);
        }
        return WebsiteDataMapper.Map(rows.Rows[0]);
    }

    public bool Exists(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        return _repository.Exists(host);
    }

    private static List<Website> Map(DataTable rows)
    {
        List<Website> websites = [];
        foreach (DataRow row in rows.Rows)
        {
            websites.Add(WebsiteDataMapper.Map(row));
        }
        return websites;
    }
}