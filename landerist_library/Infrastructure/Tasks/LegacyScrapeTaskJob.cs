using landerist_library.Application.Scraping;
using landerist_library.Application.Tasks;
using landerist_library.Scrape;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyScrapeTaskJob : IScrapeTaskJob
{
    private readonly Scraper _scraper;
    private readonly IScrapeResourceManager _resources;

    public LegacyScrapeTaskJob(
        Scraper scraper,
        IScrapeResourceManager resources)
    {
        ArgumentNullException.ThrowIfNull(scraper);
        ArgumentNullException.ThrowIfNull(resources);
        _scraper = scraper;
        _resources = resources;
    }

    public void Prepare() => _resources.UpdateChrome();

    public void Run() => _scraper.RunBatch();

    public void Stop() => _scraper.Stop();
}
