using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Scrape;
using landerist_library.Statistics;
using landerist_library.Websites;


namespace landerist_library.Tasks
{
    public class TaskBatchDownload
    {

        public static readonly HashSet<string> DownloadedPagesUriHashes = [];
        public static void Start()
        {
            var batches = Batches.SelectNonDownloaded();
            foreach (var batch in batches)
            {
                Download(batch);
            }
        }

        private static void Download(Batch batch)
        {
            DownloadedPagesUriHashes.Clear();

            var files = GetFiles(batch);
            if (files == null)
            {
                return;
            }

            if (!DownloadFile(batch, files.Value.fileSucess))
            {
                return;
            }

            RemoveWaitingStatus(batch);
            Batches.UpdateToDownloaded(batch);
        }

        private static bool DownloadFile(Batch batch, string? file)
        {
            if (file == null)
            {
                return true;
            }

            //file = "input/batch_vertexai_20250620112155_input.json";

            var filesPath = DownloadFile(batch.LLMProvider, file);
            if (filesPath == null)
            {
                return false;
            }
            var sucess = ReadFile(batch, filesPath);
            File.Delete(filesPath);
            return sucess;
        }

        private static (string? fileSucess, string? fileError)? GetFiles(Batch batch)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => OpenAIBatchDownload.GetFiles(batch.Id),
                LLMProvider.VertexAI => VertexAIBatchDownload.GetFiles(batch.Id),
                _ => null,
            };
        }

        private static string? DownloadFile(LLMProvider lLMProvider, string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                return null;
            }
            Console.WriteLine($"TaskBatchDownload {file}");
            switch (lLMProvider)
            {
                case LLMProvider.OpenAI: return OpenAIBatchClient.DownloadFile(file);
                case LLMProvider.VertexAI: return VertexAIBatchDownload.DownloadFile(file);
                default: return null;
            }
        }

        private static bool ReadFile(Batch batch, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadSucessFile(batch, lines);
                return true;
            }

            catch (Exception exception)
            {
                Log.WriteError("TaskBatchDownload ReadFile", exception);
                return false;
            }
        }

        private static void ReadSucessFile(Batch batch, string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, Config.PARALLELOPTIONS1INLOCAL, line =>
            {
                try
                {
                    if (ReadSuceesLine(batch, line))
                    {
                        Interlocked.Increment(ref readed);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("TaskBatchDownload ReadSucessFile", exception);
                    Interlocked.Increment(ref errors);
                }
            });

            Log.WriteBatch("TaskBatchDownload", $"ReadSucessFile {readed}/{lines.Length} errors: {errors}");

            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReaded, readed);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);
        }

        private static bool ReadSuceesLine(Batch batch, string line)
        {
            (Page page, string? text)? result = GetPageAndText(batch, line);
            if (result == null)
            {
                return false;
            }

            var page = result.Value.page;
            var (newPageType, listing) = ParseListing.ParseResponse(page, result.Value.text);
            if (newPageType.Equals(PageType.MayBeListing))
            {
                return false;
            }

            lock (DownloadedPagesUriHashes)
            {
                DownloadedPagesUriHashes.Add(page.UriHash);
            }

            page.RemoveWaitingStatus();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();
            new PageScraper(page).SetPageType(newPageType, listing);
            var suceess = page.Update(true);
            page.Dispose();
            return suceess;
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

        private static void RemoveWaitingStatus(Batch batch)
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
                var page = Pages.GetPage(uriHash);
                if (page != null)
                {
                    page.RemoveWaitingStatus();
                    page.RemoveResponseBodyZipped();
                    if (page.Update(false))
                    {
                        Interlocked.Increment(ref counter);
                    }
                }
            });
            Log.WriteBatch("TaskBatchDownload", $"RemoveWaitingStatus {counter}/{difference.Count}");
        }
    }
}
