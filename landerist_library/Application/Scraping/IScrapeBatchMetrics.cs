namespace landerist_library.Application.Scraping;

public interface IScrapeBatchMetrics
{
    void Record(ScrapeBatchCounters counters);
}
