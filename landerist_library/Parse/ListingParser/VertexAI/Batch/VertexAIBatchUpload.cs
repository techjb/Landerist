using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchUpload
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };
        public static string? GetJson(Page page, string userInput)
        {
            object structuredRequestData = "";
            //StructuredRequestData structuredRequestData = new()
            //{
            //    custom_id = page.UriHash,
            //    method = "POST",
            //    url = "/v1/chat/completions",
            //    body = new StructuredBody
            //    {
            //        model = OpenAIRequest.MODEL_NAME,
            //        temperature = OpenAIRequest.TEMPERATURE,
            //        messages =
            //        [
            //            new BatchMessage
            //            {
            //                role = "system",
            //                content = ParseListingRequest.GetSystemPrompt()
            //            },
            //            new BatchMessage {
            //                role = "user",
            //                content = userInput
            //            }
            //        ],
            //        response_format = new StructuredResponseFormat
            //        {
            //            type = "json_schema",
            //            json_schema = OpenAIRequest.GetOpenAIJsonSchema()
            //        },
            //    }
            //};

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
        }
    }
}
