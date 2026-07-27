using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping
{
    internal sealed class ScraperLog
    {
        private readonly IApplicationLogger _logger;
        private readonly bool _writePageProgress;
        private readonly IScrapeProgressReporter _progress;

        public ScraperLog(
            IApplicationLogger logger,
            IScrapeProgressReporter progress,
            bool writePageProgress)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(progress);
            _logger = logger;
            _progress = progress;
            _writePageProgress = writePageProgress;
        }

        public void WriteTestStart()
        {
            _logger.WriteInfo("service", "Starting test..");
        }

        public void WriteTestPageType(Page page)
        {
            _logger.WriteInfo("service", "PageType: " + page.PageType.ToString());
        }

        public void WriteTestListing(Listing? listing)
        {
            string json = new Schema(listing).Serialize();
            _logger.WriteInfo("service", "Listing: " + json);
        }

        public void WriteStart(int counter)
        {
            _progress.Write("Scrapping " + counter + " pages ..");
        }

        public void WritePage(ScrapeBatchCounters counters, Page page)
        {
            if (!_writePageProgress)
            {
                return;
            }

            _progress.Write(GetPageText(counters, page));
        }

        public void WriteTotals(ScrapeBatchCounters counters)
        {
            _logger.WriteInfo("scraper", GetTotalsText(counters));
        }

        private static string GetPageText(ScrapeBatchCounters counters, Page page)
        {
            var okPercentage = GetPercentage(counters.ScrapedSuccess, counters.Processed);
            var failedPercentage = GetPercentage(counters.Failed, counters.Total);

            return
                $"Total {counters.Total} | " +
                $"Handled {counters.Handled} ({GetPercentage(counters.Handled, counters.Total)}%) | " +
                $"Processed {counters.Processed} ({GetPercentage(counters.Processed, counters.Total)}%) | " +
                $"OK {counters.ScrapedSuccess} ({okPercentage}%) | " +
                $"Failed {counters.Failed} ({failedPercentage}%) => " +
                $"[Crash {counters.Crashed} | DlErr {counters.DownloadErrors} | Skip {counters.Skipped}] | " +
                $"{page.PageType} {page.Uri}";
        }

        private static string GetTotalsText(ScrapeBatchCounters counters)
        {
            var processedPercentage = GetPercentage(counters.Processed, counters.Total);
            var handledPercentage = GetPercentage(counters.Handled, counters.Total);
            var failedPercentage = GetPercentage(counters.Failed, counters.Total);
            var skippedPercentage = GetPercentage(counters.Skipped, counters.Total);
            var skippedByRobotsTxtPercentage = GetPercentage(counters.SkippedByRobotsTxt, counters.Total);
            var skippedByCrawlDelayPercentage = GetPercentage(counters.SkippedByCrawlDelay, counters.Total);
            var skippedByBlockedWebsitePercentage = GetPercentage(counters.SkippedByBlockedWebsite, counters.Total);
            var successPercentage = GetPercentage(counters.ScrapedSuccess, counters.Processed);
            var crashedPercentage = GetPercentage(counters.Crashed, counters.Processed);
            var downloadErrorsPercentage = GetPercentage(counters.DownloadErrors, counters.Processed);

            return
                $"Total {counters.Total} | " +
                $"Handled {counters.Handled} ({handledPercentage}%) | " +
                $"Processed {counters.Processed} ({processedPercentage}%) | " +
                $"OK {counters.ScrapedSuccess} ({successPercentage}%) | " +
                $"Failed {counters.Failed} ({failedPercentage}%) => " +
                $"[Crash {counters.Crashed} ({crashedPercentage}%) | " +
                $"DlErr {counters.DownloadErrors} ({downloadErrorsPercentage}%) | " +
                $"Skip {counters.Skipped} ({skippedPercentage}%) => " +
                $"RobotsTxt {counters.SkippedByRobotsTxt} ({skippedByRobotsTxtPercentage}%) | " +
                $"CrawlDelay {counters.SkippedByCrawlDelay} ({skippedByCrawlDelayPercentage}%) | " +
                $"Blocked {counters.SkippedByBlockedWebsite} ({skippedByBlockedWebsitePercentage}%)]";
        }

        private static double GetPercentage(int value, int total)
        {
            if (total <= 0)
            {
                return 0;
            }

            return Math.Round((double)value * 100 / total, 0);
        }
    }
}
