using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;

namespace landerist_library.Infrastructure.Scraping;

public sealed class SqlPageLockManager : IPageLockManager
{
    private readonly PageMaintenanceRepository _pages;
    private readonly string _machineName;

    public SqlPageLockManager(IDatabase database, string machineName)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(machineName);
        _pages = new PageMaintenanceRepository(database);
        _machineName = machineName;
    }

    public void CleanPageLocks() => _pages.CleanLockedBy(_machineName);

    public async Task CleanPageLocksAsync(
        CancellationToken cancellationToken = default) =>
        _ = await _pages.CleanLockedByAsync(
            _machineName,
            cancellationToken).ConfigureAwait(false);
}