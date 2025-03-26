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
            StructuredRequestData structuredRequestData = new()
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
                        response_schema = new VertexAIBatchResponseSchema()
                    }
                }
            };

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
        }
    }
}
