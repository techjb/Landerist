using landerist_library.Application.Pages;
using landerist_library.Application.Parsing;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_library.Application.Statistics;
using System.Collections.Concurrent;

namespace landerist_library.Infrastructure.Tasks
{
    public class TaskLocalAIParsing : ILocalAiParsingTask
    {
        private readonly LocalAiParsingTaskOptions _options;
        private readonly int _maxTokenCount;
        private readonly int _maxBlockingCollectionSize;
        private readonly IParsedPageClassificationService _parsedClassification;
        private readonly GlobalStatistics _globalStatistics;
        private readonly IPageWaitingStatusService _waitingStatus;
        private readonly IPageCatalog _pages;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly ILocalAiListingParser _listingParser;
        private readonly IListingInputPreparer _listingInput;
        private readonly IApplicationLogger _logger;
        private readonly Action? _reportProgress;

        private int TotalProcessed = 0;
        private int TotalErrors = 0;
        private int TotalSuccess = 0;
        private int TotalListing = 0;
        private int TotalNotListingByParser = 0;
        private readonly CancellationTokenSource StoppingCancellationTokenSource = new();
        private BlockingCollection<Page> BlockingCollection = [];

        public TaskLocalAIParsing(
            IParsedPageClassificationService parsedClassification,
            GlobalStatistics globalStatistics,
            IPageWaitingStatusService waitingStatus,
            IPageCatalog pages,
            IPagePersistenceService pagePersistence,
            ILocalAiListingParser listingParser,
            IListingInputPreparer listingInput,
            LocalAiParsingTaskOptions options,
            ILocalAiTokenBudget tokenBudget,
            IApplicationLogger logger,
            Action? reportProgress = null)
        {
            ArgumentNullException.ThrowIfNull(parsedClassification);
            ArgumentNullException.ThrowIfNull(globalStatistics);
            ArgumentNullException.ThrowIfNull(waitingStatus);
            ArgumentNullException.ThrowIfNull(pages);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(listingParser);
            ArgumentNullException.ThrowIfNull(listingInput);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(tokenBudget);
            ArgumentNullException.ThrowIfNull(logger);
            _parsedClassification = parsedClassification;
            _globalStatistics = globalStatistics;
            _waitingStatus = waitingStatus;
            _pages = pages;
            _pagePersistence = pagePersistence;
            _listingParser = listingParser;
            _listingInput = listingInput;
            _logger = logger;
            _reportProgress = reportProgress;
            _options = options;
            _maxBlockingCollectionSize = options.MaxPagesPerTask * 10;
            if (options.UpdateWaitingStatusOnStart)
            {
                _waitingStatus.Update(WaitingStatus.readed_by_localai, WaitingStatus.waiting_ai_request);
            }
            _maxTokenCount = tokenBudget.Calculate(options);
            _logger.WriteInfo(nameof(TaskLocalAIParsing), "Started");
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
                        MaxDegreeOfParallelism = _options.RunSequentially ? 1 : _options.MaxConcurrentSequences
                    },
                    page =>
                    {
                        var processPageResult = ProcessPage(page);
                        IncrementPageTypeCounter(processPageResult.PageType);

                        if (processPageResult.Success)
                        {
                            Interlocked.Increment(ref TotalSuccess);
                            _globalStatistics.InsertDailyCounter(StatisticsKey.LocalAIParsingSuccess);
                        }
                        else
                        {
                            Interlocked.Increment(ref TotalErrors);
                            _globalStatistics.InsertDailyCounter(StatisticsKey.LocalAIParsingErrors);
                        }

                        int totalProcessed = Interlocked.Increment(ref TotalProcessed);
                        _reportProgress?.Invoke();
                        if (totalProcessed % 10 == 0)
                        {
                            int totalErrors = Volatile.Read(ref TotalErrors);
                            int totalListing = Volatile.Read(ref TotalListing);
                            int totalNotListingByParser = Volatile.Read(ref TotalNotListingByParser);
                            double totalErrorPercentage = totalProcessed == 0
                                ? 0
                                : Math.Round((double)totalErrors * 100 / totalProcessed, 2);
                            double totalListingPercentage = totalProcessed == 0
                                ? 0
                                : Math.Round((double)totalListing * 100 / totalProcessed, 2);
                            double totalNotListingByParserPercentage = totalProcessed == 0
                                ? 0
                                : Math.Round((double)totalNotListingByParser * 100 / totalProcessed, 2);

                            _logger.WriteInfo(
                                "ProcessPages",
                                $"Processed: {totalProcessed} " +
                                $"Errors: {totalErrors} ({totalErrorPercentage}%) " +
                                $"Listing: {totalListing} ({totalListingPercentage}%) " +
                                $"NotListing: {totalNotListingByParser} ({totalNotListingByParserPercentage}%)");
                        }
                    });
            }
            catch (OperationCanceledException)
            {
                _logger.WriteInfo("ProcessPages", "Cancellation requested");
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
            BlockingCollection = new BlockingCollection<Page>(_maxBlockingCollectionSize);
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
                    _logger.WriteError("TaskLocalAIParsing InitializeBlockingCollection", exception.ToString());
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

            if (_maxBlockingCollectionSize < BlockingCollection.Count + _options.MaxPagesPerTask)
            {
                return true;
            }

            var pages = _waitingStatus.SelectAIRequest(_options.MaxPagesPerTask, WaitingStatus.readed_by_localai, _maxTokenCount, true);
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


        private void IncrementPageTypeCounter(PageType? pageType)
        {
            switch (pageType)
            {
                case PageType.Listing:
                    Interlocked.Increment(ref TotalListing);
                    break;
                case PageType.NotListingByParser:
                    Interlocked.Increment(ref TotalNotListingByParser);
                    break;
            }
        }

        public void ProcessPage(string uriHash)
        {
            var page = _pages.GetByHash(uriHash);
            if (page == null)
            {
                _logger.WriteError("TaskLocalAIParsing ProcessPage", "Page not found. UriHash: " + uriHash);
                return;
            }
            ProcessPage(page);
        }

        private (bool Success, PageType? PageType) ProcessPage(Page page)
        {
            bool success = false;
            PageType? newPageType = null;

            try
            {
                page.SetResponseBodyFromZipped();
                _listingInput.Prepare(page);
                string? userInput = page.ListingParserInput;
                if (string.IsNullOrEmpty(userInput))
                {
                    _logger.WriteError("TaskLocalAIParsing ProcessPage", "Error getting user input. Page: " + page.UriHash);
                    success = ReturnPageToScrape(page);
                }
                else
                {
                    var (pageType, listing, waitingAIRequest) = _listingParser.Parse(page, userInput);
                    newPageType = pageType;
                    success = _parsedClassification.Apply(page, pageType, listing);
                }
            }
            catch (Exception exception)
            {
                _logger.WriteError("TaskLocalAIParsing ProcessPage", exception.ToString());
            }
            finally
            {
                try
                {
                    if (!success)
                    {
                        _waitingStatus.UpdateAIRequest(page.UriHash);
                    }
                }
                catch (Exception exception)
                {
                    _logger.WriteError("TaskLocalAIParsing ProcessPage UpdateWaitingStatusAIRequest", exception.ToString());
                }

                page.Dispose();
            }

            return (success, newPageType);
        }

        private bool ReturnPageToScrape(Page page)
        {
            page.RemoveWaitingStatus();
            page.RemoveResponseBodyZipped();
            return _pagePersistence.Update(page);
        }
    }
}
