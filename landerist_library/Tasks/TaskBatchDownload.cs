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
            var files = GetFiles(batch);
            if (files == null)
            {
                return;
            }
            var filesPaths = DownloadFiles(batch.LLMProvider, files);
            if (filesPaths == null)
            {
                return;
            }
            if (!ReadFiles(batch, filesPaths))
            {
                return;
            }
            Batches.UpdateToDownloaded(batch.Id);
        }

        private static List<string>? GetFiles(Batch batch)
        {
            return batch.LLMProvider switch
            {
                LLMProvider.OpenAI => OpenAIBatchDownload.GetFiles(batch.Id),
                LLMProvider.VertexAI => VertexAIBatchDownload.GetFiles(batch.Id),
                _ => null,
            };
        }

        private static List<string>? DownloadFiles(LLMProvider lLMProvider, List<string> files)
        {
            List<string> filesPaths = [];
            foreach (var file in files)
            {
                if (string.IsNullOrEmpty(file))
                {
                    continue;
                }
                string? filePath = DownloadFile(lLMProvider, file);
                if (string.IsNullOrEmpty(filePath))
                {
                    return null;
                }
                filesPaths.Add(filePath);
            }
            return filesPaths;
        }

        private static string? DownloadFile(LLMProvider lLMProvider, string file)
        {
            Console.WriteLine($"TaskBatchDownload {file}");
            switch (lLMProvider)
            {
                case LLMProvider.OpenAI: return OpenAIBatchClient.DownloadFile(file);
                case LLMProvider.VertexAI: return VertexAIBatchDownload.DownloadFile(file);
                default: return null;
            }
        }

        private static bool ReadFiles(Batch batch, List<string> filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                if (!ReadFile(batch, filePath))
                {
                    return false;
                }
                File.Delete(filePath);
            }
            return true;
        }

        private static bool ReadFile(Batch batch, string filePath)
        {
            try
            {
                //Console.WriteLine($"Reading batch file: {filePath}");
                var lines = File.ReadAllLines(filePath);
                return ReadLines(batch, lines);
            }
            catch (Exception exception)
            {
                Log.WriteError("TaskBatchDownload ReadFile", exception);
                return false;
            }
        }

        private static bool ReadLines(Batch batch, string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, Config.PARALLELOPTIONS1INLOCAL, line =>
            {
                try
                {
                    if (ReadLine(batch, line))
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
                    Log.WriteError("TaskBatchDownload ReadLines", exception);
                }
            });

            int pertentage = (errors * 100) / total;
            Log.WriteInfo("batch", $"Readed {readed}/{total} Errors: {errors} ({pertentage}%)");

            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReaded, readed);
            StatisticsSnapshot.InsertDailyCounter(StatisticsKey.BatchReadedErrors, errors);

            return total > 0 && readed > 0;
        }

        private static bool ReadLine(Batch batch, string line)
        {
            (Page page, string? text)? result = GetPageAndText(batch, line);
            if (result == null)
            {
                //Console.WriteLine($"TaskBatchDownload ReadLine: Error reading line: {line}");
                return false;
            }

            var page = result.Value.page;
            var (newPageType, listing) = ParseListing.ParseResponse(page, result.Value.text);
            if (Config.IsConfigurationLocal())
            {
                page.Dispose();
                return true;
            }

            if (newPageType.Equals(PageType.MayBeListing))
            {
                //Console.WriteLine($"TaskBatchDownload ReadLine: MayBeListing");
                page.SetWaitingStatusAIRequest();
                page.Update(false);
                page.Dispose();
                return false;
            }
            page.RemoveWaitingStatus();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();
            new PageScraper(page).SetPageType(newPageType, listing);
            var sucess = page.Update(true);
            //if(!sucess)
            //{
            //    Log.WriteError("TaskBatchDownload ReadLine", "Failed to update page: " + page.UriHash);
            //}
            page.Dispose();
            return sucess;
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
    }
}
