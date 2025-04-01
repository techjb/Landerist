using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;
using landerist_library.Scrape;
using landerist_library.Websites;


namespace landerist_library.Tasks
{
    public class BatchDownload
    {
        public static void Start()
        {
            var batches = Batches.SelectNonDownloaded();
            Parallel.ForEach(batches, Config.PARALLELOPTIONS1INLOCAL, Download);
        }


        public static void Test()
        {
            Batch batch = new()
            {
                LLMProvider = LLMProvider.VertexAI,
                Id = "projects/942392546193/locations/europe-southwest1/batchPredictionJobs/3858938367819382784",
                Created = DateTime.Now,
                Downloaded = false,
            };
            Download(batch);
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
            if (!ReadFiles(batch.LLMProvider, filesPaths))
            {
                return;
            }
            Batches.UpdateToDownloaded(batch.Id);
        }

        private static List<string>? GetFiles(Batch batch)
        {
            switch (batch.LLMProvider)
            {
                case LLMProvider.OpenAI: return OpenAIBatchDownload.GetFiles(batch.Id);
                case LLMProvider.VertexAI: return VertexAIBatchDownload.GetFiles(batch.Id);
                default: return null;
            }
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
            switch (lLMProvider)
            {
                case LLMProvider.OpenAI: return OpenAIBatchClient.DownloadFile(file);
                case LLMProvider.VertexAI: return VertexAIBatchDownload.DownloadFile(file);
                default: return null;
            }
        }

        private static bool ReadFiles(LLMProvider lLMProvider, List<string> filesPaths)
        {
            foreach (var filePath in filesPaths)
            {
                if (!ReadFile(lLMProvider, filePath))
                {
                    return false;
                }
                File.Delete(filePath);
            }
            return true;
        }

        public static void ReadFileTest()
        {
            string filePath = "E:\\Landerist\\Batch\\batch_vertexai_20250327165258_output.json";
            var lines = File.ReadAllLines(filePath);
            ReadLines(LLMProvider.VertexAI, lines);
        }

        private static bool ReadFile(LLMProvider lLMProvider, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadLines(lLMProvider, lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload ReadFile", exception);
                return false;
            }
        }

        private static void ReadLines(LLMProvider lLMProvider, string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, Config.PARALLELOPTIONS1INLOCAL, line =>
            {
                try
                {
                    if (ReadLine(lLMProvider, line))
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
                    Log.WriteError("BatchDownload ReadLines", exception);
                }
            });

            int pertentage = (errors * 100) / total;
            Log.WriteInfo("batch", $"Readed {readed} Errors: {errors} ({pertentage}%)");
        }

        private static bool ReadLine(LLMProvider lLMProvider, string line)
        {
            (Page page, string? text)? result;
            switch (lLMProvider)
            {
                case LLMProvider.OpenAI: result = OpenAIBatchDownload.ReadLine(line); break;
                case LLMProvider.VertexAI: result = VertexAIBatchDownload.ReadLine(line); break;
                default: return false;
            }
            if (result == null)
            {
                return false;
            }

            var page = result.Value.page;
            var (pageType, listing) = ParseListing.ParseResponse(page, result.Value.text);
            if (pageType.Equals(PageType.MayBeListing))
            {
                page.SetWaitingAIParsingRequest();
                page.Update(false);
                return false;
            }

            page.RemoveWaitingAIParsing();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();

            new PageScraper(page).SetPageType(pageType, listing);
            var sucess = page.Update(true);
            page.Dispose();
            return sucess;
        }
    }
}
