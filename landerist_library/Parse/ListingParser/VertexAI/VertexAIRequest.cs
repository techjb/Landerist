using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using landerist_library.Configuration;
using landerist_library.Websites;
using static Google.Cloud.AIPlatform.V1.GenerationConfig.Types;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;


namespace landerist_library.Parse.ListingParser.VertexAI
{
    public class VertexAIRequest : ParseListingSystem
    {

        public const int MAX_CONTEXT_WINDOW = 128000;

        public const float Temperature = 0.2f;


        public static async Task<GenerateContentResponse?> GetResponse(string text)
        {
            try
            {
                var generateContentRequest = GetGenerateContentRequest(text);
                return await GetPredictionServiceClient().GenerateContentAsync(generateContentRequest);
            }
            catch //(Exception exception)
            {
                
            }
            return null;
        }

        public static async Task<GenerateContentResponse?> GetResponse(Page page, string text)
        {
            try
            {
                var generateContentRequest = GetGenerateContentRequest(page, text);
                DateTime dateStart = DateTime.Now;
                var response = await GetPredictionServiceClient().GenerateContentAsync(generateContentRequest);
                Timers.Timer.SaveTimerVertexAI("VertexAIRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIRequest GetAddress", exception);
            }
            return null;
        }


        private static PredictionServiceClient GetPredictionServiceClient()
        {
            return new PredictionServiceClientBuilder
            {
                Endpoint = $"{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL,
            }.Build();
        }

        public static GenerateContentRequest GetGenerateContentRequest(Page page, string text)
        {
            return new GenerateContentRequest
            {
                Model = $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}/publishers/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PUBLISHER}/models/{Config.VERTEXT_AI_MODEL_NAME_GEMINI_FLASH_LITE}",
                Contents =
                {
                    new Content()
                    {
                        Role = "USER",
                        Parts =
                        {
                            GetParts(page, text)
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = Temperature,
                    ResponseMimeType = "application/json",
                    ResponseSchema = VertexAIResponseSchema.ResponseSchema,
                    ThinkingConfig = new ThinkingConfig
                    {
                        ThinkingBudget = 0
                    }

                },
                SafetySettings =
                {
                    new SafetySetting
                    {
                        Category = HarmCategory.HateSpeech,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.DangerousContent,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Harassment,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.SexuallyExplicit,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Unspecified,
                        Threshold = HarmBlockThreshold.Off
                    },
                },
                SystemInstruction = new Content
                {
                    Parts =
                    {
                        new Part
                        {
                            Text = SystemPrompt
                        }
                    }
                },
                Labels =
                {
                    { "custom_id", page.UriHash }
                }
            };
        }

        public static GenerateContentRequest GetGenerateContentRequest(string text)
        {
            return new GenerateContentRequest
            {
                Model = $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}/publishers/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PUBLISHER}/models/{Config.VERTEXT_AI_MODEL_NAME_GEMINI_FLASH}",
                Contents =
                {
                    new Content()
                    {
                        Role = "USER",
                        Parts =
                        {
                            GetParts(text)
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = Temperature,
                    ResponseMimeType = "application/json",
                    ResponseSchema = VertexAIResponseSchema.ResponseSchema,
                    //ThinkingConfig = new ThinkingConfig
                    //{
                    //    ThinkingBudget = 0
                    //}

                },
                SafetySettings =
                {
                    new SafetySetting
                    {
                        Category = HarmCategory.HateSpeech,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.DangerousContent,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Harassment,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.SexuallyExplicit,
                        Threshold = HarmBlockThreshold.Off
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Unspecified,
                        Threshold = HarmBlockThreshold.Off
                    },
                },
                SystemInstruction = new Content
                {
                    Parts =
                    {
                        new Part
                        {
                            Text = SystemPrompt
                        }
                    }
                }
            };
        }

        private static RepeatedField<Part> GetParts(Page page, string text)
        {
            if (page.ContainsScreenshot())
            {
                return
                [
                    new Part
                        {
                            InlineData = new()
                            {
                                MimeType = "image/png",
                                Data = ByteString.CopyFrom(page.Screenshot)
                            }
                        },
                        new Part
                        {
                            Text = "Captura de pantalla"
                        },
                ];
            }
            return GetParts(text);
        }

        private static RepeatedField<Part> GetParts(string text)
        {
            return
              [
                  new Part
                    {
                        Text = text
                    }
              ];
        }
    }
}
