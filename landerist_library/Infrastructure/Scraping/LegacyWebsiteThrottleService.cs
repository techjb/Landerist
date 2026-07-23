using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Scraping;

public sealed class LegacyWebsiteThrottleService : IWebsiteThrottleService
{
    public bool Clean() => WebsitesThrottle.Clean();

    public bool IsBlocked(Website website) => WebsitesThrottle.IsBlocked(website);

    public bool TryAcquire(Website website) => WebsitesThrottle.Block(website);

    public bool ReportForbidden(Website website) => WebsitesThrottle.ReportForbidden(website);

    public bool ReportSuccess(Website website) => WebsitesThrottle.ReportSuccess(website);
}
