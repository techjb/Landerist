using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Websites;

namespace landerist_library.Tasks
{
    public class TaskBatchUpload
    {
        private enum FileWriteResult
        {
            Written,
            Skipped,
            Error
        }

        private static readonly object InitializeSync = new();

        private readonly long _maxFileSizeInBytes;
        private readonly int _maxPagesPerBatch;
        private readonly List<Page> _pages = [];
        private readonly HashSet<string> _waitingAIResponse = [];
        private readonly HashSet<string> _invalidPages = [];

        private static bool _firstTime = true;

        public TaskBatchUpload()
        {
            _maxFileSizeInBytes = SetMaxFileSize();
            _maxPagesPerBatch = GetMaxPagesPerBatch();
        }

        private static long SetMaxFileSize()
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return Config.MAX_BATCH_FILE_SIZE_OPEN_AI * 1024 * 1024;
                case LLMProvider.VertexAI:
                    return Config.MAX_BATCH_FILE_SIZE_VERTEX_AI * 1024 * 1024;
            }

            return 0;
        }

        private static int GetMaxPagesPerBatch()
        {
            if (Config.IsConfigurationLocal())
            {
                return Config.MAX_PAGES_PER_BATCH_LOCAL;
            }

            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return Config.MAX_PAGES_PER_BATCH_OPEN_AI;
                case LLMProvider.VertexAI:
                    return Config.MAX_PAGES_PER_BATCH_VERTEX_AI;
            }

            return 0;
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

        private static void Initialize()
        {
            if (!_firstTime)
            {
                return;
            }

            lock (InitializeSync)
            {
                if (!_firstTime)
                {
                    return;
                }

                Websites.Pages.UpdateWaitingStatus(WaitingStatus.readed_by_batch, WaitingStatus.waiting_ai_request);
                _firstTime = false;
            }
        }

        private bool BatchUpload()
        {
            //Pages = Websites.Pages.SelectWaitingStatusAIRequest(MaxPagesPerBatch, WaitingStatus.readed_by_batch, false);
            var tokenCount = TaskLocalAIParsing.GetMaxTokenCount();
            _pages.AddRange(Websites.Pages.SelectWaitingStatusAIRequest(_maxPagesPerBatch, WaitingStatus.readed_by_batch, tokenCount, false));

            if (_pages.Count < Config.MIN_PAGES_PER_BATCH)
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

            Batches.Insert(batchId, _waitingAIResponse);
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

            Directory.CreateDirectory(Config.BATCH_DIRECTORY!);

            var filePath = Path.Combine(
                Config.BATCH_DIRECTORY!,
                $"batch_{Config.LLM_PROVIDER.ToString().ToLowerInvariant()}_{DateTime.Now:yyyyMMddHHmmss}_input.json");

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

            if (_waitingAIResponse.Count < Config.MIN_PAGES_PER_BATCH)
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
            return writer.BaseStream.Length + sizeToAdd <= _maxFileSizeInBytes;
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
                    page.Update(false);
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

        public static string? GetJson(Page page)
        {
            page.SetResponseBodyFromZipped();
            var text = page.GetParseListingUserInput();
            page.RemoveResponseBody();

            if (string.IsNullOrEmpty(text))
            {
                Log.WriteError("TaskBatchUpload GetJson", "Error getting user input. Page: " + page.UriHash);
                return null;
            }

            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    return OpenAIBatchUpload.GetJson(page, text);
                case LLMProvider.VertexAI:
                    return VertexAIBatchUpload.GetJson(page, text);
                default:
                    return null;
            }
        }

        private static string? UploadFile(string filePath)
        {
            return Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => OpenAIBatchClient.UploadFile(filePath),
                LLMProvider.VertexAI => CloudStorage.UploadFile(filePath),
                _ => null,
            };
        }

        private static string? CreateBatch(string fileId)
        {
            return Config.LLM_PROVIDER switch
            {
                LLMProvider.OpenAI => OpenAIBatchClient.CreateBatch(fileId),
                LLMProvider.VertexAI => BatchPredictions.CreateBatch(fileId),
                _ => null,
            };
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
            if (Config.IsConfigurationLocal())
            {
                return;
            }

            if (_waitingAIResponse.Count == 0)
            {
                return;
            }

            int counter = 0;
            Parallel.ForEach(_waitingAIResponse, Config.PARALLELOPTIONS1INLOCAL, uriHash =>
            {
                if (Websites.Pages.UpdateWaitingStatusAIResponse(uriHash))
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
            Parallel.ForEach(pages, Config.PARALLELOPTIONS1INLOCAL, page =>
            {
                if (Websites.Pages.UpdateWaitingStatusAIRequest(page.UriHash))
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
            Parallel.ForEach(pages, Config.PARALLELOPTIONS1INLOCAL, page =>
            {
                if (Websites.Pages.UpdateWaitingStatusAIRequest(page.UriHash))
                {
                    Interlocked.Increment(ref counter);
                }
            });

            Log.WriteBatch("TaskBatchUpload", "SetWaitingAIRequestToAllPages: " + counter + "/" + pages.Count);
        }
    }
}
