using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;

namespace landerist_library.Tasks
{
    public class TaskBatchDownload
    {
        public readonly HashSet<string> DownloadedPagesUriHashes = [];

        public void Start()
        {
            var batches = Batches.SelectNonDownloaded();
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
            Batches.UpdateToDownloaded(batch);
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

        private static (string? fileSuccess, string? fileError)? GetFiles(Batch batch)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => OpenAIBatchDownload.GetFiles(batch.Id),
                LLMProvider.VertexAI => VertexAIBatchDownload.GetFiles(batch.Id),
                _ => null,
            };
        }

        private static string? DownloadBatchFile(LLMProvider llmProvider, string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            Console.WriteLine($"TaskBatchDownload {file}");

            return llmProvider switch
            {
                LLMProvider.OpenAI => OpenAIBatchClient.DownloadFile(file),
                LLMProvider.VertexAI => VertexAIBatchDownload.DownloadFile(file),
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

            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReaded, read);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);
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
            bool success = new PageScraper(page).ApplyParsedClassificationAfterParsing(newPageType, listing);

            if (success)
            {
                lock (DownloadedPagesUriHashes)
                {
                    DownloadedPagesUriHashes.Add(page.UriHash);
                }
            }

            return success;
        }

        private static (Page page, string? text)? GetPageAndText(Batch batch, string line)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => OpenAIBatchDownload.ReadLine(line),
                LLMProvider.VertexAI => VertexAIBatchDownload.ReadLine(batch.Id, line),
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
                    using var page = Pages.Pages.GetPage(uriHash);
                    if (page == null)
                    {
                        return;
                    }

                    page.RemoveWaitingStatus();
                    page.RemoveResponseBodyZipped();

                    if (page.Update())
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
