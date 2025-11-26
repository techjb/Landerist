using landerist_library.Configuration;
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
        private const int MAX_PAGES_PER_TASK = 100;
        private const int MAX_NUM_SEQS = 32;  // same as in localAI server
        private const int MAX_DEGREE_OF_PARALLELISM = MAX_NUM_SEQS + 20;
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
            Config.SetLLMProviderLocalAI();
            Config.EnableLogsErrorsInConsole();            
            if (Config.IsConfigurationProduction())
            {
                Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            }
            MAX_TOKEN_COUNT = GetMaxTokenCount();
            Log.Console("TaskLocalAIParsing", "Started");
        }

        public static int GetMaxTokenCount()
        {
            var systemPrompt = ParseListingSystem.GetSystemPrompt();
            int systemTokens = GptEncoding.GetEncoding(Config.LOCAL_AI_TOKENIZER).CountTokens(systemPrompt);
            return Config.LOCAL_AI_MAX_MODEL_LEN - systemTokens - COMPLETION_TOKENS;
        }

        public void ProcessPages()
        {
            InitializeBlockingCollection();
            if (BlockingCollection.Count.Equals(0))
            {
                //Console.WriteLine("No pages to process.");
                return;
            }
            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            Parallel.ForEach(orderablePartitioner,
                new ParallelOptions()
                {
                    MaxDegreeOfParallelism = Config.IsConfigurationLocal() ? 1 : MAX_DEGREE_OF_PARALLELISM
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
                    success = ReturnPageToScrape(page);                    
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

        private static bool ReturnPageToScrape(Page page)
        {
            page.RemoveWaitingStatus();
            page.RemoveResponseBodyZipped();
            return page.Update(false);
        }
    }
}
