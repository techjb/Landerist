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

    public IReadOnlySet<string> GetUrls() => _repository.GetUrls();

    public IReadOnlyList<Website> GetWithSuccessfulStatus() =>
        Map(_repository.GetHttpStatusCodeOk());

    public IReadOnlyList<Website> GetWithUnsuccessfulStatus() =>
        Map(_repository.GetHttpStatusCodeNotOk());

    public IReadOnlyList<Website> GetWithoutStatus() =>
        Map(_repository.GetHttpStatusCodeNull());

    public IReadOnlyList<Website> GetNeedingRobotsTxtUpdate(DateTime updatedBefore) =>
        Map(_repository.GetNeedToUpdateRobotsTxt(updatedBefore));

    public IReadOnlyList<Website> GetNeedingSitemapUpdate(DateTime updatedBefore) =>
        Map(_repository.GetNeedToUpdateSitemaps(updatedBefore));

    public IReadOnlyList<Website> GetNeedingIpAddressUpdate(DateTime updatedBefore) =>
        Map(_repository.GetNeedToUpdateIpAddress(updatedBefore));

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

public sealed class SqlWebsiteMaintenanceService : IWebsiteMaintenanceService
{
    private readonly WebsiteQueryRepository _repository;

    public SqlWebsiteMaintenanceService(WebsiteQueryRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public bool DeleteAll() => _repository.DeleteAll();
}
