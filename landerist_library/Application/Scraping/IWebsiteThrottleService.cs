using landerist_library.Websites;

namespace landerist_library.Application.Scraping;

public interface IWebsiteThrottleService
{
    bool Clean();

    bool IsBlocked(Website website);

    bool TryAcquire(Website website);

    bool ReportForbidden(Website website);

    bool ReportSuccess(Website website);
}
