using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Scrape;
using landerist_library.Websites;
using OpenAI;
using OpenAI.Batch;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchDownload: BatchClient
    {        

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static void Start()
        {
            var batchIds = Batches.SelectNonDownloaded();
            Parallel.ForEach(batchIds,                 
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                batchId =>
            {
                //Console.WriteLine(batchId);
                //if (!batchId.Equals("batch_678fa99488d88190922bbc2336a856a0"))
                //{
                //    return;
                //}
                var batchResponse = GetBatch(batchId);
                if (batchResponse == null || !BatchIsCompleted(batchResponse))
                {
                    return;
                }
                Download(batchResponse);
            });
        }

        //public static void Test()
        //{
        //    var batch = GetBatch("batch_672c297160b081909856773d7211b236");
        //}

        
        private static void Download(BatchResponse batchResponse)
        {
            if (Download(batchResponse.OutputFileId) && Download(batchResponse.ErrorFileId))
            {
                Batches.UpdateToDownloaded(batchResponse.Id);
            }
        }

        private static bool Download(string? fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                return true;
            }
            var filePath = DownloadFile(fileId);
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            bool sucess = ReadFile(filePath);
            File.Delete(filePath);
            return sucess;
        }
       

        private static bool ReadFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                ReadLines(lines);
                return true;
            }
            catch (Exception exception)
            {
                Log.WriteError("BatchDownload ReadFile", exception);
                return false;
            }
        }

        private static void ReadLines(string[] lines)
        {
            int total = lines.Length;
            int readed = 0;
            int errors = 0;

            Parallel.ForEach(lines, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                //MaxDegreeOfParallelism = 1
            }, line =>
            {
                try
                {
                    if (ReadLine(line))
                    {
                        Interlocked.Increment(ref readed);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                        Log.WriteError("BatchDownload ReadLines. Error reading line: ", line);
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("BatchDownload ReadLines", exception);
                }
            });

            //Log.WriteInfo("BatchDownload", $"{readed}/{total}, Errors: {errors}");
            Log.WriteInfo("batch", $"Downloaded {readed}");
        }

        private static bool ReadLine(string line)
        {
            var batchResponseLine = JsonSerializer.Deserialize<BatchLineResponse?>(line, JsonSerializerOptions);

            if (batchResponseLine == null)
            {
                Log.WriteError("BatchDownload ReadLine", "batchResponse is null. Line: " + line);
                return false;
            }

            var page = Pages.GetPage(batchResponseLine.CustomId);
            if (page == null)
            {
                Log.WriteError("BatchDownload ReadLine", "Page is null. CustomId: " + batchResponseLine.CustomId);
                return false;
            }

            if (!batchResponseLine.Response.StatusCode.Equals(200))
            {
                Log.WriteError("BatchDownload ReadLine", "Not 200 StatusCode. CustomId: " + batchResponseLine.CustomId);
                page.SetWaitingAIParsingRequest();
                return page.Update(false);
            }

            page.RemoveWaitingAIParsing();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();

            var (pageType, listing) = ParseListing.ParseOpenAI(page, batchResponseLine.Response.Body);
            var sucess = new PageScraper(page).SetPageType(pageType, listing);
            page.Dispose();
            return sucess;
        }
    }
}
