using landerist_library.Websites;
using landerist_library.Application.Pages;
using landerist_library.Application.Parsing;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_library.Application.Statistics;
using landerist_library.Application.Websites;

namespace landerist_library.Infrastructure.Tasks
{
    public sealed class TaskBatchDownload
    {
        private readonly IParsedPageClassificationService _parsedClassification;
        private readonly IBatchStore _batches;
        private readonly GlobalStatistics _statistics;
        private readonly IPageCatalog _pages;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly BatchDownloadProviderCatalog _providers;
        private readonly IBatchListingResponseParser _listingParser;
        private readonly BatchDownloadOptions _options;
        private readonly IApplicationLogger _logger;

        public TaskBatchDownload(
            IParsedPageClassificationService parsedClassification,
            IBatchStore batches,
            GlobalStatistics statistics,
            IPageCatalog pages,
            IPagePersistenceService pagePersistence,
            BatchDownloadProviderCatalog providers,
            IBatchListingResponseParser listingParser,
            BatchDownloadOptions options,
            IApplicationLogger logger)
        {
            ArgumentNullException.ThrowIfNull(parsedClassification);
            ArgumentNullException.ThrowIfNull(batches);
            ArgumentNullException.ThrowIfNull(statistics);
            ArgumentNullException.ThrowIfNull(pages);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(providers);
            ArgumentNullException.ThrowIfNull(listingParser);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(logger);
            _parsedClassification = parsedClassification;
            _batches = batches;
            _statistics = statistics;
            _pages = pages;
            _pagePersistence = pagePersistence;
            _providers = providers;
            _listingParser = listingParser;
            _options = options;
            _logger = logger;
        }

        public readonly HashSet<string> DownloadedPagesUriHashes = [];

        public void Start()
        {
            var batches = _batches.Select(downloaded: false);
            foreach (var batch in batches)
            {
                Download(batch);
            }
        }

        private void Download(BatchRecord batch)
        {
            DownloadedPagesUriHashes.Clear();

            var files = GetFiles(batch);
            if (files == null)
            {
                return;
            }

            var (fileSuccess, _) = files.Value;
            if (!DownloadAndReadFile(batch, fileSuccess))
            {
                return;
            }

            RemoveWaitingStatus(batch);
            _batches.MarkDownloaded(batch.Id);
        }

        private bool DownloadAndReadFile(BatchRecord batch, string? file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return true;
            }

            //file = "input/batch_vertexai_20250620112155_input.json";

            var filePath = DownloadBatchFile(batch.Provider, file);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            try
            {
                return ReadFile(batch, filePath);
            }
            finally
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch (Exception exception)
                {
                    _logger.WriteError("TaskBatchDownload DeleteFile", exception.ToString());
                }
            }
        }

        private (string? fileSuccess, string? fileError)? GetFiles(BatchRecord batch) =>
            _providers.GetRequired(batch.Provider).GetFiles(batch.Id);

        private string? DownloadBatchFile(BatchProvider provider, string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            _logger.WriteInfo("TaskBatchDownload", file);
            return _providers.GetRequired(provider).DownloadFile(file);
        }
        private bool ReadFile(BatchRecord batch, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadSuccessFile(batch, lines);
                return true;
            }
            catch (Exception exception)
            {
                _logger.WriteError("TaskBatchDownload ReadFile", exception.ToString());
                return false;
            }
        }

        private void ReadSuccessFile(BatchRecord batch, string[] lines)
        {
            int read = 0;
            int errors = 0;

            Parallel.ForEach(lines, _options.ParallelOptions, line =>
            {
                try
                {
                    if (ReadSuccessLine(batch, line))
                    {
                        Interlocked.Increment(ref read);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
                catch (Exception exception)
                {
                    _logger.WriteError("TaskBatchDownload ReadSuccessFile", exception.ToString());
                    Interlocked.Increment(ref errors);
                }
            });

            _logger.WriteInfo("TaskBatchDownload", $"ReadSuccessFile {read}/{lines.Length} errors: {errors}");

            _statistics.InsertDailyCounter(StatisticsKey.BatchReaded, read);
            _statistics.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);
        }

        private bool ReadSuccessLine(BatchRecord batch, string line)
        {
            var result = GetPageAndText(batch, line);
            if (result == null)
            {
                return false;
            }

            using var page = result.Value.page;
            var text = result.Value.text;

            var (newPageType, listing) = _listingParser.Parse(page, text, batch.Provider);
            bool success = _parsedClassification.Apply(page, newPageType, listing);

            if (success)
            {
                lock (DownloadedPagesUriHashes)
                {
                    DownloadedPagesUriHashes.Add(page.UriHash);
                }
            }

            return success;
        }

        private (Page page, string? text)? GetPageAndText(BatchRecord batch, string line) =>
            _providers.GetRequired(batch.Provider).ReadLine(batch.Id, line, _pages);

        private void RemoveWaitingStatus(BatchRecord batch)
        {
            var difference = new HashSet<string>(batch.PageUriHashes);
            difference.ExceptWith(DownloadedPagesUriHashes);

            if (difference.Count == 0)
            {
                return;
            }

            int counter = 0;

            Parallel.ForEach(difference, _options.ParallelOptions, uriHash =>
            {
                try
                {
                    using var page = _pages.GetByHash(uriHash);
                    if (page == null)
                    {
                        return;
                    }

                    page.RemoveWaitingStatus();
                    page.RemoveResponseBodyZipped();

                    if (_pagePersistence.Update(page))
                    {
                        Interlocked.Increment(ref counter);
                    }
                }
                catch (Exception exception)
                {
                    _logger.WriteError("TaskBatchDownload RemoveWaitingStatus", exception.ToString());
                }
            });

            _logger.WriteInfo("TaskBatchDownload", $"RemoveWaitingStatus {counter}/{difference.Count}");
        }
    }
}
