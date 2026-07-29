using landerist_library.Application.Scraping;
using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class ScrapeTaskJob : IScrapeTaskJob
{
    private readonly Scraper _scraper;
    private readonly IScrapeBrowserManager _browser;

    public ScrapeTaskJob(
        Scraper scraper,
        IScrapeBrowserManager browser)
    {
        ArgumentNullException.ThrowIfNull(scraper);
        ArgumentNullException.ThrowIfNull(browser);
        _scraper = scraper;
        _browser = browser;
    }

    public void Prepare() => _browser.UpdateChrome();

    public void Run() => _scraper.RunBatch();

    public void Stop() => _scraper.Stop();

    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _scraper.StopAsync(cancellationToken);
}
