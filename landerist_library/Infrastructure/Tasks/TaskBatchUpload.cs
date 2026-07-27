using landerist_library.Application.Pages;
using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Infrastructure.Tasks
{
    public class TaskBatchUpload
    {
        private enum FileWriteResult
        {
            Written,
            Skipped,
            Error
        }

        private readonly object _initializeSync = new();

        private readonly BatchRepository _batches;
        private readonly IPageWaitingStatusService _waitingStatus;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly BatchUploadOptions _options;
        private readonly IListingBatchUploadProvider _provider;
        private readonly TimeProvider _timeProvider;
        private readonly ParallelOptions _statusParallelOptions;
        private readonly List<Page> _pages = [];
        private readonly HashSet<string> _waitingAIResponse = [];
        private readonly HashSet<string> _invalidPages = [];

        private bool _initialized;

        public TaskBatchUpload(
            BatchRepository batches,
            IPageWaitingStatusService waitingStatus,
            IPagePersistenceService pagePersistence,
            BatchUploadOptions options,
            ListingBatchUploadProviderCatalog providers,
            TimeProvider timeProvider)
        {
            ArgumentNullException.ThrowIfNull(batches);
            ArgumentNullException.ThrowIfNull(waitingStatus);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(providers);
            ArgumentNullException.ThrowIfNull(timeProvider);
            _batches = batches;
            _waitingStatus = waitingStatus;
            _pagePersistence = pagePersistence;
            _options = options;
            _provider = providers.GetRequired(options.Provider);
            _timeProvider = timeProvider;
            _statusParallelOptions = options.CreateStatusParallelOptions();
        }

        public void Start()
        {
            Initialize();

            try
            {
                while (true)
                {
                    Clear();

                    var success = BatchUpload();
                    if (!success)
                    {
                        break;
                    }

                    if (_waitingAIResponse.Count >= _pages.Count)
                    {
                        break;
                    }
                }
            }
            finally
            {
                Clear();
            }
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            lock (_initializeSync)
            {
                if (_initialized)
                {
                    return;
                }

                _waitingStatus.Update(WaitingStatus.readed_by_batch, WaitingStatus.waiting_ai_request);
                _initialized = true;
            }
        }

        private bool BatchUpload()
        {
            var tokenCount = _options.MaxInputTokens;
            _pages.AddRange(_waitingStatus.SelectAIRequest(_options.MaxPagesPerBatch, WaitingStatus.readed_by_batch, tokenCount, false));

            if (_pages.Count < _options.MinPagesPerBatch)
            {
                SetWaitingAIRequestToAllPages();
                return false;
            }

            var filePath = CreateFile();
            if (string.IsNullOrEmpty(filePath))
            {
                SetWaitingAIRequestToAllPages();
                return false;
            }

            var fileId = UploadFile(filePath);
            if (string.IsNullOrEmpty(fileId))
            {
                SetWaitingAIRequestToAllPages();
                return false;
            }

            var batchId = CreateBatch(fileId);
            if (string.IsNullOrEmpty(batchId))
            {
                SetWaitingAIRequestToAllPages();
                return false;
            }

            _batches.Insert(batchId, _waitingAIResponse, _options.Provider);
            SetWaitingAIResponse();
            SetWaitingAIRequest();
            return true;
        }

        private string? CreateFile()
        {
            if (_pages.Count == 0)
            {
                return null;
            }

            Directory.CreateDirectory(_options.Directory);

            var filePath = Path.Combine(
                _options.Directory,
                $"batch_{_options.Provider.ToString().ToLowerInvariant()}_{_timeProvider.GetLocalNow():yyyyMMddHHmmss}_input.json");

            Console.WriteLine("TaskBatchUpload " + filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            _waitingAIResponse.Clear();

            var errors = 0;
            var skipped = 0;
            var stopProcessing = false;

            using StreamWriter writer = new(filePath, append: false)
            {
                AutoFlush = true
            };

            for (var index = 0; index < _pages.Count; index++)
            {
                var result = WriteToFile(_pages[index], writer);
                switch (result)
                {
                    case FileWriteResult.Written:
                        break;
                    case FileWriteResult.Skipped:
                        skipped = _pages.Count - index;
                        stopProcessing = true;
                        break;
                    default:
                        errors++;
                        break;
                }

                if (stopProcessing)
                {
                    break;
                }
            }

            Log.WriteBatch("TaskBatchUpload", $"CreateFile {_waitingAIResponse.Count}/{_pages.Count} skipped: {skipped} errors: {errors}");

            if (_waitingAIResponse.Count < _options.MinPagesPerBatch)
            {
                File.Delete(filePath);
                return null;
            }

            return filePath;
        }

        private bool CanWriteFile(StreamWriter writer, string json)
        {
            writer.Flush();
            var sizeToAdd = writer.Encoding.GetByteCount(json + Environment.NewLine);
            return writer.BaseStream.Length + sizeToAdd <= _options.MaxFileSizeInBytes;
        }

        private FileWriteResult WriteToFile(Page page, StreamWriter writer)
        {
            try
            {
                var json = GetJson(page);
                if (string.IsNullOrEmpty(json))
                {
                    _invalidPages.Add(page.UriHash);
                    page.RemoveWaitingStatus();
                    _pagePersistence.Update(page);
                    return FileWriteResult.Error;
                }

                if (!CanWriteFile(writer, json))
                {
                    return FileWriteResult.Skipped;
                }

                writer.WriteLine(json);
                _waitingAIResponse.Add(page.UriHash);
                return FileWriteResult.Written;
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskBatchUpload AddToBatch", exception.Message);
            }

            return FileWriteResult.Error;
        }

        private string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
            var text = page.GetListingParserInput();
            page.RemoveResponseBody();

            if (string.IsNullOrEmpty(text))
            {
                Log.WriteError("TaskBatchUpload GetJson", "Error getting user input. Page: " + page.UriHash);
                return null;
            }

            return _provider.Serialize(page, text);
        }

        private string? UploadFile(string filePath) =>
            _provider.UploadFile(filePath);

        private string? CreateBatch(string fileId) =>
            _provider.CreateBatch(fileId);

        private void Clear()
        {
            foreach (var page in _pages)
            {
                page.Dispose();
            }

            _pages.Clear();
            _waitingAIResponse.Clear();
            _invalidPages.Clear();
        }

        private void SetWaitingAIResponse()
        {
            if (!_options.UpdateWaitingResponse)
            {
                return;
            }

            if (_waitingAIResponse.Count == 0)
            {
                return;
            }

            int counter = 0;
            Parallel.ForEach(_waitingAIResponse, _statusParallelOptions, uriHash =>
            {
                if (_waitingStatus.UpdateAIResponse(uriHash))
                {
                    Interlocked.Increment(ref counter);
                }
            });

            Log.WriteBatch("TaskBatchUpload", "SetWaitingAIResponse: " + counter + "/" + _waitingAIResponse.Count);
        }

        private void SetWaitingAIRequest()
        {
            var pages = _pages
                .Where(page => !_waitingAIResponse.Contains(page.UriHash) && !_invalidPages.Contains(page.UriHash))
                .ToList();

            if (pages.Count == 0)
            {
                return;
            }

            int counter = 0;
            Parallel.ForEach(pages, _statusParallelOptions, page =>
            {
                if (_waitingStatus.UpdateAIRequest(page.UriHash))
                {
                    Interlocked.Increment(ref counter);
                }
            });

            Log.WriteBatch("TaskBatchUpload", "SetWaitingAIRequest: " + counter + "/" + pages.Count);
        }

        private void SetWaitingAIRequestToAllPages()
        {
            var pages = _pages
                .Where(page => !_invalidPages.Contains(page.UriHash))
                .ToList();

            if (pages.Count == 0)
            {
                return;
            }

            int counter = 0;
            Parallel.ForEach(pages, _statusParallelOptions, page =>
            {
                if (_waitingStatus.UpdateAIRequest(page.UriHash))
                {
                    Interlocked.Increment(ref counter);
                }
            });

            //Log.WriteBatch("TaskBatchUpload", "SetWaitingAIRequestToAllPages: " + counter + "/" + pages.Count);
        }
    }
}
