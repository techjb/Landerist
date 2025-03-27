using landerist_library.Database;
using landerist_library.Logs;
using landerist_library.Scrape;
using landerist_library.Websites;
using OpenAI.Batch;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class OpenAIBatchDownload : OpenAIBatchClient
    {

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        public static void BatchDownload(string batchId)
        {
            var batchResponse = GetBatch(batchId);
            if (batchResponse == null || !BatchIsCompleted(batchResponse))
            {
                return;
            }
            Download(batchResponse);
        }


        private static void Download(BatchResponse batchResponse)
        {
            if (Download(batchResponse.OutputFileId) && Download(batchResponse.ErrorFileId))
            {
                Batches.UpdateToDownloaded(batchResponse.Id);
                //DeleteFile(batchResponse.OutputFileId);
                //DeleteFile(batchResponse.ErrorFileId);
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
                Log.WriteError("OpenAIBatchDownload ReadFile", exception);
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
                    }
                }
                catch (Exception exception)
                {
                    Log.WriteError("OpenAIBatchDownload ReadLines", exception);
                }
            });

            Log.WriteInfo("batch", $"Downloaded {readed} Errors: " + errors);
        }

        private static bool ReadLine(string line)
        {
            OpenAIBatchResponse? batchResponseLine;
            try
            {
                batchResponseLine = JsonSerializer.Deserialize<OpenAIBatchResponse?>(line, JsonSerializerOptions);
            }
            catch (Exception exception)
            {
                // todo: set page to WaitingAIParsing = 1
                Log.WriteError("OpenAIBatchDownload ReadLine Serialization", exception);
                return false;
            }

            if (batchResponseLine == null)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "batchResponse is null. Line: " + line);
                return false;
            }

            var page = Pages.GetPage(batchResponseLine.CustomId);
            if (page == null)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "Page is null. CustomId: " + batchResponseLine.CustomId);
                return false;
            }

            if (!batchResponseLine.Response.StatusCode.Equals(200))
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "Not 200 StatusCode. CustomId: " + batchResponseLine.CustomId);
                page.SetWaitingAIParsingRequest();
                return page.Update(false);
            }

            page.RemoveWaitingAIParsing();
            page.SetResponseBodyFromZipped();
            page.RemoveResponseBodyZipped();

            var sucess = false;
            try
            {
                var (pageType, listing) = ParseListing.ParseOpenAI(page, batchResponseLine.Response.Body);
                sucess = new PageScraper(page).SetPageType(pageType, listing);
            }
            catch (Exception exception)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine ParseListing", exception);
            }
            page.Dispose();
            return sucess;
        }
    }
}
