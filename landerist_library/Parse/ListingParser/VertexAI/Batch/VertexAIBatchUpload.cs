using System.Text.Json;
using landerist_library.Websites;
using landerist_library.Parse.ListingParser.StructuredOutputs;

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
            var responseSchema = OpenApiSchemaSerializer.Serialize(VertexAIResponseSchema.ResponseSchema);

            VertexAIBatchRequest structuredRequestData = new()
            {
                request = new Request()
                {
                    contents =
                    [
                        new Content
                        {
                            role = "user",
                            parts =
                            [
                                new Part
                                {
                                    text = userInput
                                }
                            ]
                        }
                    ],
                    system_instruction = new SystemInstruction
                    {
                        parts =
                        [
                            new Part
                            {
                                text = ParseListingRequest.GetSystemPrompt()
                            }
                        ]
                    },
                    generation_config = new GenerationConfig
                    {
                        temperature = 0f,
                        response_mime_type = "application/json",
                        response_schema = responseSchema
                    },
                    labels = new Dictionary<string, string>()
                    {
                        {"custom_id", page.UriHash}
                    }
                }
            };

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
        }
    }
}
