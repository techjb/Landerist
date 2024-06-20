using AI.Dev.OpenAI.GPT;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using landerist_library.Configuration;
using landerist_library.Websites;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;


namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIRequest : ParseListingRequest
    {

        public const int MAX_CONTEXT_WINDOW = 128000;

        private const string GEMINI_FLASH = "gemini-1.5-flash";

        private const string GEMINI_PRO = "gemini-1.5-pro";

        private static readonly string ModelName =
            GEMINI_FLASH;
        //GEMINI_PRO;                            

        private static readonly HarmBlockThreshold HarmBlockThreshold = HarmBlockThreshold.BlockOnlyHigh;


        public static bool TooManyTokens(Page page)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemPrompt).Count;
            string? text = UserTextInput.GetText(page);
            if (text == null)
            {
                return false;
            }
            int userTokens = GPT3Tokenizer.Encode(text).Count;
            int totalTokens = systemTokens + userTokens;
            return totalTokens > MAX_CONTEXT_WINDOW;
        }

        public static async Task<GenerateContentResponse?> GetResponse(string text)
        {
            try
            {
                var predictionServiceClient = GetPredictionServiceClient();
                var generateContentRequest = GetGenerateContentRequest(text);
                DateTime dateStart = DateTime.Now;
                var response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);
                Timers.Timer.SaveTimerVertexAI("VertexAIRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("VertexAIRequest GetResponse", exception);
            }
            return null;
        }

       
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseScreenshot(Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            string? text = ParseScreenshot(page.Screenshot).Result;
            Console.WriteLine(text);

            return result;
        }

        private static async Task<string?> ParseScreenshot(byte[] screenshot)
        {
            try
            {
                var predictionServiceClient = GetPredictionServiceClient();
                var generateContentRequest = GetGenerateContentRequest(screenshot);
                GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);
                var functionCall = response.Candidates[0].Content.Parts[0].FunctionCall;
                return response.Candidates[0].Content.Parts[0].Text;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("VertexAIRequest ParseScreenshot", exception);
            }
            return null;
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseScreenShot(Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            string? text = ParseScreenshot(page.Screenshot).Result;
            Console.WriteLine(text);

            return result;
        }

        private static PredictionServiceClient GetPredictionServiceClient()
        {
            return new PredictionServiceClientBuilder
            {
                Endpoint = $"{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL,
            }.Build();
        }

        private static GenerateContentRequest GetGenerateContentRequest(string text)
        {
            var content = new Content
            {
                Role = "USER",
                Parts =
                    {
                        new Part
                        {
                            Text = text
                        }
                    }
            };
            return GetGenerateContentRequest(content);
        }

        private static GenerateContentRequest GetGenerateContentRequest(byte[] screenshot)
        {
            var content = new Content
            {
                Role = "USER",
                Parts =
                    {
                        new Part
                        {
                            InlineData = new()
                            {
                                MimeType = "image/png",
                                Data = ByteString.CopyFrom(screenshot)
                            }
                        },
                        new Part
                        {
                            Text = "Captura de pantalla"
                        },
                    }
            };
            return GetGenerateContentRequest(content);
        }

        private static GenerateContentRequest GetGenerateContentRequest(Content? content)
        {
            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}/publishers/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PUBLISHER}/models/{ModelName}",
                Contents =
                {
                    content
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = 0f,                    
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
                },
                Tools =
                {
                    VertexAITools.GetTools()
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

            // ToolConfig only supported in gemini 1.5 pro
            //https://cloud.google.com/vertex-ai/generative-ai/docs/multimodal/function-calling#tool-config
            if (ModelName.Equals(GEMINI_PRO))
            {
                generateContentRequest.ToolConfig = new ToolConfig
                {
                    FunctionCallingConfig = new FunctionCallingConfig
                    {
                        Mode = FunctionCallingConfig.Types.Mode.Any,
                    }
                };
            }

            return generateContentRequest;
        }
    }
}
