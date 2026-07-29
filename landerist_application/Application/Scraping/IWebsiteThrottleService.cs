using landerist_library.Websites;

namespace landerist_library.Application.Scraping;

public interface IWebsiteThrottleService
{
    bool Clean();

    Task<bool> CleanAsync(CancellationToken cancellationToken = default);

    bool IsBlocked(Website website);

    bool TryAcquire(Website website);

    bool ReportForbidden(Website website);

    bool ReportSuccess(Website website);
}
