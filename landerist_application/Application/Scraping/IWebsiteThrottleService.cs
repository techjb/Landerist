using landerist_library.Websites;

namespace landerist_library.Application.Scraping;

public interface IWebsiteThrottleService
{
    bool Clean();

    Task<bool> CleanAsync(CancellationToken cancellationToken = default);

    bool IsBlocked(Website website);

    Task<bool> IsBlockedAsync(
        Website website,
        CancellationToken cancellationToken = default);

    bool TryAcquire(Website website);

    Task<bool> TryAcquireAsync(
        Website website,
        CancellationToken cancellationToken = default);

    bool ReportForbidden(Website website);

    bool ReportSuccess(Website website);
}
