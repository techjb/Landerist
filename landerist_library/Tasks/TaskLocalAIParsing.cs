using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;
using SharpToken;
using System.Collections.Concurrent;

namespace landerist_library.Tasks
{
    public class TaskLocalAIParsing
    {
        private const int MAX_PAGES_PER_TASK = 50;
        private const int MAX_MODEL_LEN = 18000; // same as in localAI server
        private const int COMPLETION_TOKENS = 5000;  // structured output and completion tokens aproximately        
        private readonly int MAX_TOKEN_COUNT;

        private static int TotalProcessed = 0;
        private static int TotalErrors = 0;
        private static int TotalSucess = 0;
        private BlockingCollection<Page> BlockingCollection = [];
        private const int MAX_SIZE_BLOCKINGCOLLECTION = MAX_PAGES_PER_TASK * 10;
        //private readonly DateTime StartDate = DateTime.UtcNow.ToLocalTime();


        public TaskLocalAIParsing()
        {
            Configuration.Config.SetLLMProviderLocalAI();
            //Configuration.Config.EnableLogsErrorsInConsole();
            Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            MAX_TOKEN_COUNT = GetMaxTokenCount();
        }

        public static int GetMaxTokenCount()
        {
            var systemPrompt = ParseListingSystem.GetSystemPrompt();
            var systemTokens = GptEncoding.GetEncoding(Configuration.Config.LOCAL_AI_TOKENIZER).CountTokens(systemPrompt);
            return MAX_MODEL_LEN - systemTokens - COMPLETION_TOKENS;
        }

        public void ProcessPages()
        {
            Log.Console("TaskLocalAIParsing", "Processing pages ..");
            InitializeBlockingCollection();
            if (BlockingCollection.Count.Equals(0))
            {
                Log.WriteLocalAI("ProcessPages", $"No pages to process.");
                return;
            }
            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(orderablePartitioner,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Configuration.Config.IsConfigurationLocal() ? 1 : 8
                },
                page =>
                {
                    if (ProcessPage(page))
                    {
                        Interlocked.Increment(ref TotalSucess);
                        StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingSuccess);
                    }
                    else
                    {
                        Interlocked.Increment(ref TotalErrors);
                        StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingErrors);
                    }
                    Interlocked.Increment(ref TotalProcessed);
                    if (TotalProcessed % 10 == 0)
                    {
                        double totalErrorPercentage = TotalProcessed == 0 ? 0 : (int)Math.Round((double)TotalErrors * 100 / TotalProcessed, 2);                        
                        //Log.WriteLocalAI("ProcessPages", $"Errors: {TotalErrors}/{TotalProcessed} ({totalErrorPercentage}%) Daily estimate: " + DailyEstimate());
                        Log.WriteLocalAI("ProcessPages", $"Errors: {TotalErrors}/{TotalProcessed} ({totalErrorPercentage}%)");
                    }
                });
        }

        //private int DailyEstimate()
        //{
        //    if (TotalProcessed <= 0)
        //    {
        //        return 0;
        //    }
        //    TimeSpan timeSpan = DateTime.Now - StartDate;
        //    double days = timeSpan.TotalDays;
        //    if (days <= 0)
        //    {
        //        days = TimeSpan.FromHours(1).TotalDays;
        //    }

        //    return (int)Math.Round(TotalProcessed / days);
        //}


        private void InitializeBlockingCollection()
        {
            BlockingCollection = new BlockingCollection<Page>(MAX_SIZE_BLOCKINGCOLLECTION);
            if (!AddPagesToBlockingCollection())
            {
                BlockingCollection.CompleteAdding();
                return;
            }
            var producerTask = Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        Thread.Sleep(10000);
                        if (!AddPagesToBlockingCollection())
                        {
                            break;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("TaskLocalAIParsing InitializeBlockingCollection", exception);
                }
                finally
                {
                    BlockingCollection.CompleteAdding();
                }
            });
        }

        private bool AddPagesToBlockingCollection()
        {
            if (MAX_SIZE_BLOCKINGCOLLECTION < BlockingCollection.Count + MAX_PAGES_PER_TASK)
            {
                return true;
            }
            var pages = Pages.SelectWaitingStatusAIRequest(MAX_PAGES_PER_TASK, WaitingStatus.readed_by_localai, MAX_TOKEN_COUNT, true);
            if (pages.Count.Equals(0))
            {
                return false;
            }
            foreach (var page in pages)
            {
                BlockingCollection.Add(page);
            }
            return true;
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
