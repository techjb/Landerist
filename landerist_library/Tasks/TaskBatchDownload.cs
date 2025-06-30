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

            if (!DownloadFile(batch, files.Value.fileSucess, true))
            {
                return;
            }

            if (!DownloadFile(batch, files.Value.fileError, false))
            {
                return;
            }
            SetNonDownloadedPagesToWaitingAIRequest(batch);
            Batches.UpdateToDownloaded(batch);
        }

        private static bool DownloadFile(Batch batch, string? file, bool isSucessFile)
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
            var sucess = ReadFile(batch, filesPath, isSucessFile);
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

        private static bool ReadFile(Batch batch, string filePath, bool isSucessFile)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                if (isSucessFile)
                {
                    ReadSucessFile(batch, lines);
                }
                else
                {
                    ReadErrorFile(batch, lines);
                }
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
                }
            });

            //int percentage = (errors * 100) / total;
            Log.WriteInfo("batch", $"Downloaded {readed} errors: {errors}");

            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReaded, readed);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);
        }

        private static void ReadErrorFile(Batch batch, string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, Config.PARALLELOPTIONS1INLOCAL, line =>
            {
                try
                {
                    if (ReadErrorLine(batch, line))
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
                    Log.WriteError("TaskBatchDownload ReadErrorFile", exception);
                }
            });

            int percentage = (errors * 100) / total;
            Log.WriteInfo("batch", $"ReadErrorFile {readed}/{total} errors: {errors} ({percentage}%)");

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
            DownloadedPagesUriHashes.Add(page.UriHash);
            var (newPageType, listing) = ParseListing.ParseResponse(page, result.Value.text);
            bool suceess = false;
            if (newPageType.Equals(PageType.MayBeListing))
            {
                page.SetWaitingStatusAIRequest();
                suceess = page.Update(false);
            }
            else
            {
                page.RemoveWaitingStatus();
                page.SetResponseBodyFromZipped();
                page.RemoveResponseBodyZipped();
                new PageScraper(page).SetPageType(newPageType, listing);
                suceess = page.Update(true);
            }
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

        private static bool ReadErrorLine(Batch batch, string line)
        {
            var page = GetPage(batch, line);
            if (page == null)
            {
                Log.WriteError("TaskBatchDownload ReadErrorLine", "Page is null for line: " + line);
                return false;
            }
            page.SetWaitingStatusAIRequest();
            var sucees = page.Update(false);
            page.Dispose();
            return sucees;
        }

        private static Page? GetPage(Batch batch, string line)
        {
            return batch.LLMProvider switch
            {
                //LLMProvider.OpenAI => OpenAIBatchDownload.ReadLine(line),
                LLMProvider.VertexAI => VertexAIBatchUpload.GetPage(line),
                _ => null,
            };
        }

        private static void SetNonDownloadedPagesToWaitingAIRequest(Batch batch)
        {
            var difference = new HashSet<string>(batch.PagesUriHashes);
            difference.ExceptWith(DownloadedPagesUriHashes);
            if (difference.Count == 0)
            {
                return;
            }
            int counter = 0;
            foreach (string uriHash in difference)
            {
                var page = Pages.GetPage(uriHash);
                if (page != null)
                {
                    page.SetWaitingStatusAIRequest();
                    if (page.Update(false))
                    {
                        counter++;
                    }
                }
            }
            Log.WriteInfo("batch", $"SetNonDownloadedPagesToWaitingAIRequest {counter}/{difference.Count}.");
        }
    }
}
