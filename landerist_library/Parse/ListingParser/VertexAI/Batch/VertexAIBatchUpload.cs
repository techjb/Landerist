﻿using Google.Cloud.AIPlatform.V1;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Websites;
using System.Text.Json;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchUpload
    {
        public const string LABEL_URIHASH = "uri_hash";

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false
        };
        public static string? GetJson(Page page, string userInput)
        {
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
                                text = ParseListingSystem.GetSystemPrompt()
                            }
                        ]
                    },
                    generation_config = new GenerationConfig
                    {
                        temperature = VertexAIRequest.Temperature,
                        response_mime_type = "application/json",
                        response_schema = OpenApiSchemaSerializer.Serialize(VertexAIResponseSchema.ResponseSchema),
                        thinking_config = new ThinkingConfig
                        {
                            thinking_budget = 0,
                        }
                    },
                    safety_settings =
                    [
                        new SafetySetting
                        {
                            category = (int)HarmCategory.HateSpeech,
                            threshold = (int)HarmBlockThreshold.BlockOnlyHigh
                        },
                        new SafetySetting
                        {
                            category = (int)HarmCategory.DangerousContent,
                            threshold = (int)HarmBlockThreshold.BlockOnlyHigh
                        },
                        new SafetySetting
                        {
                            category = (int)HarmCategory.Harassment,
                            threshold = (int)HarmBlockThreshold.BlockOnlyHigh
                        },
                        new SafetySetting
                        {
                            category = (int)HarmCategory.SexuallyExplicit,
                            threshold = (int)HarmBlockThreshold.BlockOnlyHigh
                        },
                        new SafetySetting
                        {
                            category = (int)HarmCategory.Unspecified,
                            threshold = (int)HarmBlockThreshold.BlockOnlyHigh
                        },
                    ],
                    labels = new Dictionary<string, string>()
                    {
                        {LABEL_URIHASH, page.UriHash}
                    }
                }
            };

            return JsonSerializer.Serialize(structuredRequestData, JsonSerializerOptions);
        }

        public static Page? GetPage(string json)
        {
            var vertexAIBatchRequest = JsonSerializer.Deserialize<VertexAIBatchRequest>(json);
            if (vertexAIBatchRequest is null || vertexAIBatchRequest.request is null)
            {
                return null;
            }
            var uriHash = vertexAIBatchRequest.request.labels[LABEL_URIHASH];
            if (string.IsNullOrEmpty(uriHash))
            {
                return null;
            }
            return Pages.GetPage(uriHash);
        }
    }
}
