using landerist_library.Websites;
using System.Text.Json;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class OpenAIBatchUpload : OpenAIBatchClient
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };

        public static string? GetJson(Page page, string userInput)
        {
            StructuredRequestData structuredRequestData = new()
            {
                custom_id = page.UriHash,
                method = "POST",
                url = "/v1/chat/completions",
                body = new StructuredBody
                {
                    model = OpenAIRequest.MODEL_NAME,
                    temperature = OpenAIRequest.TEMPERATURE,
                    messages =
                    [
                        new BatchMessage
                        {
                            role = "system",
                            content = ParseListingSystem.GetSystemPrompt()
                        },
                        new BatchMessage {
                            role = "user",
                            content = userInput
                        }
                    ],
                    response_format = new StructuredResponseFormat
                    {
                        type = "json_schema",
                        json_schema = OpenAIRequest.OpenAIJsonSchema
                    },
                }
            };

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
        }
    }
}
