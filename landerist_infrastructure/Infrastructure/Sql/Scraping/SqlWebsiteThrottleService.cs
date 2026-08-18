using landerist_library.Application.Scraping;
using landerist_library.Application.Websites;
using landerist_library.Database;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Sql.Scraping;

public sealed class SqlWebsiteThrottleService : IWebsiteThrottleService
{
    private readonly WebsitesThrottle _throttle;

    public SqlWebsiteThrottleService(IDatabase database, IWebsiteRobotsPolicy robots)
    {
        _throttle = new WebsitesThrottle(database, robots);
    }

    public bool Clean() => _throttle.Clean();

    public Task<bool> CleanAsync(CancellationToken cancellationToken = default) =>
        _throttle.CleanAsync(cancellationToken);

    public bool IsBlocked(Website website) => _throttle.IsBlocked(website);

    public Task<bool> IsBlockedAsync(
        Website website,
        CancellationToken cancellationToken = default) =>
        _throttle.IsBlockedAsync(website, cancellationToken);

    public bool TryAcquire(Website website) => _throttle.Block(website);

    public Task<bool> TryAcquireAsync(
        Website website,
        CancellationToken cancellationToken = default) =>
        _throttle.BlockAsync(website, cancellationToken);

    public bool ReportForbidden(Website website) => _throttle.ReportForbidden(website);

    public Task<bool> ReportForbiddenAsync(
        Website website,
        CancellationToken cancellationToken = default) =>
        _throttle.ReportForbiddenAsync(website, cancellationToken);

    public bool ReportSuccess(Website website) => _throttle.ReportSuccess(website);

    public Task<bool> ReportSuccessAsync(
        Website website,
        CancellationToken cancellationToken = default) =>
        _throttle.ReportSuccessAsync(website, cancellationToken);
}
