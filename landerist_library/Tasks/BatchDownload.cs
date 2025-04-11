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
        private static int ErrorResultNull = 0;
        private static int ErrorParseListingResponse = 0;
        private static int ErrroSetPageType = 0;

        public static void Start()
        {
            var batches = Batches.SelectNonDownloaded();
            Parallel.ForEach(batches, Config.PARALLELOPTIONS1INLOCAL, Download);
        }


        public static void DownloadVertexAI(string id)
        {
            Batch batch = new()
            {
                LLMProvider = LLMProvider.VertexAI,
                Id = id,
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
            if (!ReadFiles(batch, filesPaths))
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
                var lines = File.ReadAllLines(filePath);
                ReadLines(batch, lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload ReadFile", exception);
                return false;
            }
        }

        private static void ReadLines(Batch batch, string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;
            ErrorResultNull = 0;
            ErrorParseListingResponse = 0;
            ErrroSetPageType = 0;

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
                    Log.WriteError("BatchDownload ReadLines", exception);
                }
            });

            int pertentage = (errors * 100) / total;
            Log.WriteInfo("batch", $"Readed {readed} Errors: {errors} ({pertentage}%) ErrorResultNull: {ErrorResultNull} ErrorParseListingResponse: {ErrorParseListingResponse} ErrroSetPageType: {ErrroSetPageType}");
        }

        private static bool ReadLine(Batch batch, string line)
        {
            (Page page, string? text)? result = GetPageAndText(batch, line);            
            if (result == null)
            {
                Interlocked.Increment(ref ErrorResultNull);   
                return false;
            }

            var page = result.Value.page;
            var (pageType, listing) = ParseListing.ParseResponse(page, result.Value.text);
            if (pageType.Equals(PageType.MayBeListing))
            {
                Interlocked.Increment(ref ErrorParseListingResponse);
                page.SetWaitingAIParsingRequest();
                page.Update(false);
                page.Dispose();
                return false;
            }

            page.RemoveWaitingAIParsing();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();

            new PageScraper(page).SetPageType(pageType, listing);
            var sucess = page.Update(true);
            page.Dispose();
            if(!sucess)
            {
                Interlocked.Increment(ref ErrroSetPageType);
            }            
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
