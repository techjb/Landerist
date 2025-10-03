using Amazon.Auth.AccessControlPolicy;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Tasks
{
    public class TaskLocalAIParsing
    {
        private const int MaxPagesPerTask = 50;

        private bool FirstTime = true;
        private static int TotalProcessed = 0;
        private static int TotalErrors = 0;
        public void Start()
        {
            Initialize();
            var pages = Pages.SelectWaitingStatusAIRequest(MaxPagesPerTask, WaitingStatus.readed_by_localai);
            ProcessPages(pages);
        }

        private void Initialize()
        {
            if (!FirstTime)
            {
                return;
            }

            Configuration.Config.SetLLMProviderLocalAI();
            Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            FirstTime = false;
            TotalProcessed = 0;
            TotalErrors = 0;

            Log.WriteLocalAI("Initialize", "LocalAIParsing initialized");
        }

        private static void ProcessPages(List<Page> pages)
        {
            if (pages.Count.Equals(0))
            {
                return;
            }
            int total = pages.Count;
            int sucess = 0;
            int errors = 0;
            int counter = 0;
            Log.Console($"Processing {total} pages ..");
            Parallel.ForEach(pages,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Configuration.Config.IsConfigurationLocal() ? 1 : 8
                },
                page =>
            {
                Interlocked.Increment(ref counter);
                //vConsole.WriteLine($"Processing {counter}/{total}");
                if (ProcessPage(page))
                {
                    Interlocked.Increment(ref sucess);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
            });

            TotalProcessed += total;
            TotalErrors += errors;

            int errorPercentage = TotalProcessed == 0 ? 0 : (int)Math.Round((double)TotalErrors * 100 / TotalProcessed, 2);

            Log.WriteLocalAI("ProcessPages", $"{sucess}/{total} Processed: {TotalProcessed} Errors: {TotalErrors} ({errorPercentage} %) ");
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingSuccess, sucess);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingErrors, errors);
        }

        private static bool ProcessPage(Page page)
        {
            bool success = false;
            try
            {
                page.SetResponseBodyFromZipped();
                var userInput = ParseListingUserInput.GetText(page);
                if (string.IsNullOrEmpty(userInput))
                {
                    Log.WriteError("TaskLocalAIParsing ProcessPage", "Error getting user input. Page: " + page.UriHash);
                }
                else
                {
                    (PageType newPageType, Listing? listing, bool waitingAIRequest) = ParseListing.ParseLocalAI(page, userInput);
                    success = new PageScraper(page).SetPageTypeAfterParsing(newPageType, listing);
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskLocalAIParsing ProcessPage", exception);
            }
            if (!success)
            {
                Pages.UpdateWaitingStatusAIRequest(page.UriHash);
            }
            page.Dispose();
            return success;
        }
    }
}
