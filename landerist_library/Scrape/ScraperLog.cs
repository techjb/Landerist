using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    internal class ScraperLogCounters
    {
        public int Total { get; init; }

        public int Processed { get; init; }

        public int ScrapedSuccess { get; init; }

        public int Crashed { get; init; }

        public int DownloadErrors { get; init; }

        public int SkippedByRobotsTxt { get; init; }

        public int SkippedByCrawlDelay { get; init; }

        public int SkippedByBlockedWebsite { get; init; }

        public int Skipped => SkippedByRobotsTxt + SkippedByCrawlDelay + SkippedByBlockedWebsite;

        public int Handled => Processed + Skipped;

        public int Failed => Crashed + DownloadErrors + Skipped;
    }

    internal static class ScraperLog
    {
        public static void WriteTestStart()
        {
            Log.WriteInfo("service", "Starting test..");
        }

        public static void WriteTestPageType(Page page)
        {
            Log.WriteInfo("service", "PageType: " + page.PageType.ToString());
        }

        public static void WriteTestListing(Listing? listing)
        {
            string json = new Schema(listing).Serialize();
            Log.WriteInfo("service", "Listing: " + json);
        }

        public static void WriteStart(int counter)
        {
            Console.WriteLine("Scrapping " + counter + " pages ..");
        }

        public static void WritePage(ScraperLogCounters counters, Page page)
        {
            if (Config.IsConfigurationProduction())
            {
                return;
            }

            Console.WriteLine(GetPageText(counters, page));
        }

        public static void WriteTotals(ScraperLogCounters counters)
        {
            Log.WriteInfo("scraper", GetTotalsText(counters));
        }

        private static string GetPageText(ScraperLogCounters counters, Page page)
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

        private static string GetTotalsText(ScraperLogCounters counters)
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
