using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using Google.Protobuf.Collections;
using landerist_library.Configuration;
using landerist_library.Websites;
using System.Reflection.Emit;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;


namespace landerist_library.Parse.ListingParser.VertexAI
{
    public class VertexAIRequest : ParseListingRequest
    {

        public const int MAX_CONTEXT_WINDOW = 128000;

        private const string GEMINI_FLASH = "gemini-2.0-flash-001";

        private const string GEMINI_PRO = "gemini-2.0-pro-exp-02-05";

        public static readonly string ModelName = GEMINI_FLASH; //GEMINI_PRO;                            

        private static readonly HarmBlockThreshold HarmBlockThreshold = HarmBlockThreshold.BlockOnlyHigh;


        public static bool TooManyTokens(Page page)
        {
            return TooManyTokens(page, MAX_CONTEXT_WINDOW);
        }

        public static async Task<GenerateContentResponse?> GetResponse(Page page, string text)
        {
            try
            {
                var predictionServiceClient = GetPredictionServiceClient();
                var generateContentRequest = GetGenerateContentRequest(page, text);
                DateTime dateStart = DateTime.Now;
                var response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);
                Timers.Timer.SaveTimerVertexAI("VertexAIRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIRequest GetResponse", exception);
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
            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}/publishers/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PUBLISHER}/models/{ModelName}",                
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
                    Temperature = 0f,
                    ResponseMimeType = "application/json",
                    ResponseSchema = VertexAIResponseSchema.ResponseSchema
                },
                SafetySettings =
                {
                    new SafetySetting
                    {
                        Category = HarmCategory.HateSpeech,
                        Threshold = HarmBlockThreshold
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.DangerousContent,
                        Threshold = HarmBlockThreshold
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Harassment,
                        Threshold = HarmBlockThreshold
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.SexuallyExplicit,
                        Threshold = HarmBlockThreshold
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.CivicIntegrity,
                        Threshold = HarmBlockThreshold
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Unspecified,
                        Threshold = HarmBlockThreshold
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
            return generateContentRequest;
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
