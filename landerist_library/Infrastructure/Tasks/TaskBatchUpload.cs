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
        private readonly object _initializeSync = new();

        private readonly BatchRepository _batches;
        private readonly IPageWaitingStatusService _waitingStatus;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly BatchUploadOptions _options;
        private readonly IListingBatchUploadProvider _provider;
        private readonly IBatchInputWriter _inputWriter;
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
            IBatchInputWriter inputWriter)
        {
            ArgumentNullException.ThrowIfNull(batches);
            ArgumentNullException.ThrowIfNull(waitingStatus);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(providers);
            ArgumentNullException.ThrowIfNull(inputWriter);
            _batches = batches;
            _waitingStatus = waitingStatus;
            _pagePersistence = pagePersistence;
            _options = options;
            _provider = providers.GetRequired(ToBatchProvider(options.Provider));
            _inputWriter = inputWriter;
            _statusParallelOptions = options.CreateStatusParallelOptions();
        }

        private static BatchProvider ToBatchProvider(LLMProvider provider) =>
            provider switch
            {
                LLMProvider.OpenAI => BatchProvider.OpenAI,
                LLMProvider.VertexAI => BatchProvider.VertexAI,
                _ => throw new InvalidOperationException(
                    $"Batch upload is not supported for {provider}.")
            };

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

            BatchInputWriteResult writeResult = _inputWriter.Write(_pages);
            _waitingAIResponse.UnionWith(writeResult.WrittenPageHashes);
            _invalidPages.UnionWith(writeResult.InvalidPageHashes);
            PersistInvalidPages();
            string? filePath = writeResult.FilePath;
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

        private string? UploadFile(string filePath) =>
            _provider.UploadFile(filePath);

        private string? CreateBatch(string fileId) =>
            _provider.CreateBatch(fileId);

        private void PersistInvalidPages()
        {
            foreach (Page page in _pages.Where(
                page => _invalidPages.Contains(page.UriHash)))
            {
                page.RemoveWaitingStatus();
                _pagePersistence.Update(page);
            }
        }
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
