using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;
using SharpToken;

namespace landerist_library.Tasks
{
    public class TaskLocalAIParsing
    {
        private const int MAX_PAGES_PER_TASK = 50;
        private const int MAX_MODEL_LEN = 18000; // same as in localAI server
        private const int COMPLETION_TOKENS = 5000;  // structured output and completion tokens aproximately        
        private int MAX_TOKEN_COUNT;        

        private bool FirstTime = true;
        private static int TotalProcessed = 0;
        private static int TotalErrors = 0;

        public void Start()
        {
            Initialize();
            var pages = Pages.SelectWaitingStatusAIRequest(MAX_PAGES_PER_TASK, WaitingStatus.readed_by_localai, MAX_TOKEN_COUNT, true);

            ProcessPages(pages);
        }

        private void Initialize()
        {
            if (!FirstTime)
            {
                return;
            }

            Configuration.Config.SetLLMProviderLocalAI();
            //Configuration.Config.EnableLogsErrorsInConsole();
            Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            FirstTime = false;
            TotalProcessed = 0;
            TotalErrors = 0;

            var systemPrompt = ParseListingSystem.GetSystemPrompt();
            var systemTokens = GptEncoding.GetEncoding(Configuration.Config.LOCAL_AI_TOKENIZER).CountTokens(systemPrompt);
            MAX_TOKEN_COUNT = GetMaxTokenCount();

            Log.Console("Initialize", "LocalAIParsing");
        }

        public static int GetMaxTokenCount()
        {
            var systemPrompt = ParseListingSystem.GetSystemPrompt();
            var systemTokens = GptEncoding.GetEncoding(Configuration.Config.LOCAL_AI_TOKENIZER).CountTokens(systemPrompt);
            return MAX_MODEL_LEN - systemTokens - COMPLETION_TOKENS;
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
            Log.WriteLocalAI("ProcessPages", $"{total} pages ..");
            Parallel.ForEach(pages,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Configuration.Config.IsConfigurationLocal() ? 1 : 8
                },
                page =>
            {
                Interlocked.Increment(ref counter);                                
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

            int errorPercentage = total == 0 ? 0 : (int)Math.Round((double)errors * 100 / total, 2);
            int totalErrorPercentage = TotalProcessed == 0 ? 0 : (int)Math.Round((double)TotalErrors * 100 / TotalProcessed, 2);

            Log.WriteLocalAI("ProcessPages", $"Errors: {errors}/{total}({errorPercentage}%) Total: {TotalErrors}/{TotalProcessed} ({totalErrorPercentage} %) ");
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingSuccess, sucess);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingErrors, errors);
        }

        private static bool ProcessPage(Page page)
        {
            bool success = false;
            try
            {
                page.SetResponseBodyFromZipped();
                var userInput = page.GetParseListingUserInput();
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
