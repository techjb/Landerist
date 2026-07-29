namespace landerist_library.Application.Scraping;

public interface IPageLockManager
{
    void CleanPageLocks();

    Task CleanPageLocksAsync(CancellationToken cancellationToken = default);
}