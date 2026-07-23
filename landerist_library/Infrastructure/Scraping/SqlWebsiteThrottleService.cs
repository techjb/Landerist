using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlWebsiteThrottleService : IWebsiteThrottleService
{
    private readonly WebsitesThrottle _throttle;

    public SqlWebsiteThrottleService(IDatabase database)
    {
        _throttle = new WebsitesThrottle(database);
    }

    public bool Clean() => _throttle.Clean();

    public bool IsBlocked(Website website) => _throttle.IsBlocked(website);

    public bool TryAcquire(Website website) => _throttle.Block(website);

    public bool ReportForbidden(Website website) => _throttle.ReportForbidden(website);

    public bool ReportSuccess(Website website) => _throttle.ReportSuccess(website);
}
