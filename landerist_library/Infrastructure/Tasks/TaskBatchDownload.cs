using landerist_library.Infrastructure.Parsing;
using landerist_library.Configuration;
using landerist_library.Application.Pages;
using landerist_library.Application.Parsing;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Infrastructure.Parsing.OpenAI;
using landerist_library.Infrastructure.Parsing.VertexAI;
using landerist_library.Application.Statistics;

namespace landerist_library.Infrastructure.Tasks
{
    public class TaskBatchDownload
    {
        private readonly IParsedPageClassificationService _parsedClassification;
        private readonly BatchRepository _batches;
        private readonly GlobalStatistics _statistics;
        private readonly IPageCatalog _pages;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly IListingBatchProvider _openAi;
        private readonly IListingBatchProvider _vertexAi;

        public TaskBatchDownload(
            IParsedPageClassificationService parsedClassification,
            BatchRepository batches,
            GlobalStatistics statistics,
            IPageCatalog pages,
            IPagePersistenceService pagePersistence,
            IListingBatchProvider openAi,
            IListingBatchProvider vertexAi)
        {
            ArgumentNullException.ThrowIfNull(parsedClassification);
            ArgumentNullException.ThrowIfNull(batches);
            ArgumentNullException.ThrowIfNull(statistics);
            ArgumentNullException.ThrowIfNull(pages);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(openAi);
            ArgumentNullException.ThrowIfNull(vertexAi);
            _parsedClassification = parsedClassification;
            _batches = batches;
            _statistics = statistics;
            _pages = pages;
            _pagePersistence = pagePersistence;
            _openAi = openAi;
            _vertexAi = vertexAi;
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

        private void Download(Batch batch)
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
            _batches.Update(batch.Id, downloaded: true);
        }

        private bool DownloadAndReadFile(Batch batch, string? file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return true;
            }

            //file = "input/batch_vertexai_20250620112155_input.json";

            var filePath = DownloadBatchFile(batch.LLMProvider, file);
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
                    Log.WriteError("TaskBatchDownload DeleteFile", exception);
                }
            }
        }

        private (string? fileSuccess, string? fileError)? GetFiles(Batch batch)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => _openAi.GetFiles(batch.Id),
                LLMProvider.VertexAI => _vertexAi.GetFiles(batch.Id),
                _ => null,
            };
        }

        private string? DownloadBatchFile(LLMProvider llmProvider, string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            Console.WriteLine($"TaskBatchDownload {file}");

            return llmProvider switch
            {
                LLMProvider.OpenAI => _openAi.DownloadFile(file),
                LLMProvider.VertexAI => _vertexAi.DownloadFile(file),
                _ => null,
            };
        }

        private bool ReadFile(Batch batch, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadSuccessFile(batch, lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskBatchDownload ReadFile", exception);
                return false;
            }
        }

        private void ReadSuccessFile(Batch batch, string[] lines)
        {
            int read = 0;
            int errors = 0;

            Parallel.ForEach(lines, Config.PARALLELOPTIONS1INLOCAL, line =>
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
                    Log.WriteError("TaskBatchDownload ReadSuccessFile", exception);
                    Interlocked.Increment(ref errors);
                }
            });

            Log.WriteBatch("TaskBatchDownload", $"ReadSuccessFile {read}/{lines.Length} errors: {errors}");

            _statistics.InsertDailyCounter(StatisticsKey.BatchReaded, read);
            _statistics.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);
        }

        private bool ReadSuccessLine(Batch batch, string line)
        {
            var result = GetPageAndText(batch, line);
            if (result == null)
            {
                return false;
            }

            using var page = result.Value.page;
            var text = result.Value.text;

            var (newPageType, listing) = ParseListing.ParseResponse(page, text);
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

        private (Page page, string? text)? GetPageAndText(Batch batch, string line)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => _openAi.ReadLine(batch.Id, line, _pages),
                LLMProvider.VertexAI => _vertexAi.ReadLine(batch.Id, line, _pages),
                _ => null,
            };
        }

        private void RemoveWaitingStatus(Batch batch)
        {
            var difference = new HashSet<string>(batch.PagesUriHashes);
            difference.ExceptWith(DownloadedPagesUriHashes);

            if (difference.Count == 0)
            {
                return;
            }

            int counter = 0;

            Parallel.ForEach(difference, Config.PARALLELOPTIONS1INLOCAL, uriHash =>
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
                    Log.WriteError("TaskBatchDownload RemoveWaitingStatus", exception);
                }
            });

            Log.WriteBatch("TaskBatchDownload", $"RemoveWaitingStatus {counter}/{difference.Count}");
        }
    }
}
