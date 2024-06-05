using AI.Dev.OpenAI.GPT;
using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using landerist_library.Configuration;
using landerist_library.Websites;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;


namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIRequest
    {

        public const int MAX_CONTEXT_WINDOW = 128000;

        private static readonly string ModelName =
                            "gemini-1.5-flash";
        //"gemini-1.5-pro";

        private static readonly string ProjectId = "landerist";

        private static readonly string Location = "europe-southwest1";

        private static readonly string Publisher = "google";

        public static readonly string SystemPrompt =
           "Tu tarea consiste en procesar el html proporcionado por el usuario, identificando si corresponde a una página web de un anuncio inmobiliario. " +
           "De ser así, deberás analizar meticulosamente el contenido para determinar que efectivamente se trata de un único anuncio y proceder a extraer los datos relevantes.  " +
           "Asegúrate de tener una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
           "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

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

        public static (string?, string?) GetFunctionNameAndArguments(GenerateContentResponse response)
        {
            try
            {
                return (response.Candidates[0].Content.Parts[0].FunctionCall.Name,
                    response.Candidates[0].Content.Parts[0].FunctionCall.Args.ToString());
            }
            catch(Exception exception)
            {
                Logs.Log.WriteLogErrors("VertexAIRequest GetFunctionNameAndArguments", exception);
            }
            return (null, null);
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
                Endpoint = $"{Location}-aiplatform.googleapis.com",
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
            return new GenerateContentRequest
            {
                Model = $"projects/{ProjectId}/locations/{Location}/publishers/{Publisher}/models/{ModelName}",
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
                        Threshold = HarmBlockThreshold.BlockOnlyHigh
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.DangerousContent,
                        Threshold = HarmBlockThreshold.BlockOnlyHigh
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.Harassment,
                        Threshold = HarmBlockThreshold.BlockOnlyHigh
                    },
                    new SafetySetting
                    {
                        Category = HarmCategory.SexuallyExplicit,
                        Threshold = HarmBlockThreshold.BlockOnlyHigh
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
                },
                // only supported in gemini 1.5 pro
                //ToolConfig = new ToolConfig
                //{
                //    FunctionCallingConfig = new FunctionCallingConfig
                //    {
                //        Mode = FunctionCallingConfig.Types.Mode.Any,
                //    }
                //}
            };
        }
    }
}
