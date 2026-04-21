using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Scrape;
using landerist_library.Statistics;
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

        private int TotalProcessed = 0;
        private int TotalErrors = 0;
        private int TotalSuccess = 0;
        private readonly CancellationTokenSource StoppingCancellationTokenSource = new();
        private BlockingCollection<Page> BlockingCollection = [];
        private const int MAX_SIZE_BLOCKINGCOLLECTION = MAX_PAGES_PER_TASK * 10;
        //private readonly DateTime StartDate = DateTime.UtcNow.ToLocalTime();


        public TaskLocalAIParsing()
        {
            Config.SetLLMProviderLocalAI();
            Config.EnableLogsErrorsInConsole();
            if (Config.IsConfigurationProduction())
            {
                Pages.Pages.UpdateWaitingStatus(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            }
            MAX_TOKEN_COUNT = GetMaxTokenCount();
            Log.Console("TaskLocalAIParsing", "Started");
        }

        public static int GetMaxTokenCount()
        {
            var systemPrompt = ParseListingSystem.GetSystemPrompt();
            int systemTokens = GptEncoding.GetEncoding(Config.LOCAL_AI_TOKENIZER).CountTokens(systemPrompt);
            var otherTokens = systemTokens + COMPLETION_TOKENS;
            return Config.LOCAL_AI_MAX_MODEL_LEN - otherTokens;
        }

        public void ProcessPages(CancellationToken cancellationToken = default)
        {
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(StoppingCancellationTokenSource.Token, cancellationToken);
            CancellationToken linkedCancellationToken = linkedCancellationTokenSource.Token;

            InitializeBlockingCollection(linkedCancellationToken);
            if (BlockingCollection.Count == 0)
            {
                return;
            }

            var orderablePartitioner = Partitioner.Create(BlockingCollection.GetConsumingEnumerable(), EnumerablePartitionerOptions.NoBuffering);
            try
            {
                Parallel.ForEach(orderablePartitioner,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Config.IsConfigurationLocal() ? 1 : MAX_DEGREE_OF_PARALLELISM
                    },
                    page =>
                    {
                        if (ProcessPage(page))
                        {
                            Interlocked.Increment(ref TotalSuccess);
                            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingSuccess);
                        }
                        else
                        {
                            Interlocked.Increment(ref TotalErrors);
                            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.LocalAIParsingErrors);
                        }

                        int totalProcessed = Interlocked.Increment(ref TotalProcessed);
                        if (totalProcessed % 10 == 0)
                        {
                            int totalErrors = Volatile.Read(ref TotalErrors);
                            double totalErrorPercentage = totalProcessed == 0
                                ? 0
                                : Math.Round((double)totalErrors * 100 / totalProcessed, 2);

                            Log.WriteLocalAI("ProcessPages", $"Errors: {totalErrors}/{totalProcessed} ({totalErrorPercentage}%)");
                        }
                    });
            }
            catch (OperationCanceledException)
            {
                Log.WriteLocalAI("ProcessPages", "Cancellation requested");
            }
        }

        public void Stop()
        {
            if (StoppingCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            StoppingCancellationTokenSource.Cancel();

            if (!BlockingCollection.IsAddingCompleted)
            {
                BlockingCollection.CompleteAdding();
            }
        }

        private void InitializeBlockingCollection(CancellationToken cancellationToken)
        {
            BlockingCollection = new BlockingCollection<Page>(MAX_SIZE_BLOCKINGCOLLECTION);
            if (!AddPagesToBlockingCollection(cancellationToken))
            {
                BlockingCollection.CompleteAdding();
                return;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(10000, cancellationToken);
                        if (!AddPagesToBlockingCollection(cancellationToken))
                        {
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception exception)
                {
                    Log.WriteError("TaskLocalAIParsing InitializeBlockingCollection", exception);
                }
                finally
                {
                    BlockingCollection.CompleteAdding();
                }
            }, cancellationToken);
        }

        private bool AddPagesToBlockingCollection(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (MAX_SIZE_BLOCKINGCOLLECTION < BlockingCollection.Count + MAX_PAGES_PER_TASK)
            {
                return true;
            }

            var pages = Pages.Pages.SelectWaitingStatusAIRequest(MAX_PAGES_PER_TASK, WaitingStatus.readed_by_localai, MAX_TOKEN_COUNT, true);
            if (pages.Count == 0)
            {
                return false;
            }

            foreach (var page in pages)
            {
                if (BlockingCollection.IsAddingCompleted)
                {
                    return false;
                }

                BlockingCollection.Add(page, cancellationToken);
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
                    success = new PageScraper(page).ApplyParsedClassificationAfterParsing(newPageType, listing);
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskLocalAIParsing ProcessPage", exception);
            }
            finally
            {
                try
                {
                    if (!success)
                    {
                        Pages.Pages.UpdateWaitingStatusAIRequest(page.UriHash);
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("TaskLocalAIParsing ProcessPage UpdateWaitingStatusAIRequest", exception);
                }

                page.Dispose();
            }

            return success;
        }

        private static bool ReturnPageToScrape(Page page)
        {
            page.RemoveWaitingStatus();
            page.RemoveResponseBodyZipped();
            return page.Update();
        }
    }
}
