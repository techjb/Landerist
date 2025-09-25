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
        private const int MaxPagesPerTask = 10;

        private bool FirstTime = true;
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

            Log.WriteLocalAI("Initialize", "Initialized");
            Configuration.Config.SetLLMProviderLocalAI();
            Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            FirstTime = false;
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
                //new ParallelOptions() { MaxDegreeOfParallelism = 4 }, 
                //new ParallelOptions() { MaxDegreeOfParallelism = 8 },
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
            Log.WriteLocalAI("ProcessPages", $"Total {total} Success: {sucess} Errors: {errors}");
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
