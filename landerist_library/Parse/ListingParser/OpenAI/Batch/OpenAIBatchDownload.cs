using landerist_library.Logs;
using landerist_library.Websites;
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

        public static List<string>? GetFiles(string batchId)
        {
            var batchResponse = GetBatch(batchId);
            if (batchResponse == null || !BatchIsCompleted(batchResponse))
            {
                return null;
            }

            return [batchResponse.OutputFileId, batchResponse.ErrorFileId];
        }

        public static (Page page, string? text)? ReadLine(string line)
        {
            OpenAIBatchResponse? batchResponseLine;
            try
            {
                batchResponseLine = JsonSerializer.Deserialize<OpenAIBatchResponse?>(line, JsonSerializerOptions);
            }
            catch (Exception exception)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine Serialization", exception);
                return null;
            }

            if (batchResponseLine == null)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "batchResponse is null. Line: " + line);
                return null;
            }

            var page = Pages.GetPage(batchResponseLine.CustomId);
            if (page == null)
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "Page is null. CustomId: " + batchResponseLine.CustomId);
                return null;
            }

            string? text = null;
            if (!batchResponseLine.Response.StatusCode.Equals(200) || 
                batchResponseLine.Response == null ||
                batchResponseLine.Response.Body == null ||
                batchResponseLine.Response.Body.FirstChoice == null
                )
            {
                Log.WriteError("OpenAIBatchDownload ReadLine", "Not 200 StatusCode. CustomId: " + batchResponseLine.CustomId);                
            }
            else
            {
                text = batchResponseLine.Response.Body.FirstChoice;
            } 

            return (page, text);
        }
    }
}
