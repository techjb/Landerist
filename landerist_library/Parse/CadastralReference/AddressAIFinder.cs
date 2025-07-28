using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.VertexAI;
using Newtonsoft.Json;
using static Google.Cloud.AIPlatform.V1.GenerationConfig.Types;
using static Google.Cloud.AIPlatform.V1.SafetySetting.Types;



namespace landerist_library.Parse.CadastralReference
{
    public class AddressAIFinder(string address, List<string> addressList)
    {
        const string SystemPrompt = "Propósito y metas:\r\n\r\n\r\n\r\n* Ayudar a los usuarios a encontrar la dirección postal no estructurada que coincide exactamente con una dirección buscada, dentro de un listado de direcciones equivalentes.\r\n\r\n* Responder únicamente con la dirección equivalente que coincide, o con 'null' si no se encuentra ninguna coincidencia exacta.\r\n\r\n\r\n\r\nComportamientos y reglas:\r\n\r\n\r\n\r\n1) Procesamiento de la entrada:\r\n\r\na) El usuario proporcionará dos elementos: una 'dirección buscada' (no estructurada) y un 'listado de direcciones postales no estructuradas equivalentes'.\r\n\r\nb) Debes analizar ambas entradas para identificar la dirección exacta en el listado que se corresponde con la dirección buscada.\r\n\r\nc) La coincidencia debe ser exacta. Considera variaciones menores como mayúsculas/minúsculas o espacios extra, pero prioriza la correspondencia literal. Si hay diferencias sustanciales (ej. número de calle, nombre de calle, ciudad diferente), no es una coincidencia exacta. Ignora todo aquello que tiene que ver con la identificación dentro de la finca, como por ejemplo el número de portal, el número de piso o local.\r\n\r\n\r\n\r\n2) Salida:\r\n\r\na) Si encuentras una coincidencia exacta en el listado, responde únicamente con esa dirección del listado. No incluyas ningún otro texto, explicaciones o preámbulos.\r\n\r\nb) Si no encuentras ninguna dirección en el listado que se corresponda exactamente con la dirección buscada, responde únicamente con la palabra 'null' (en minúsculas). No incluyas ningún otro texto.\r\n\r\n\r\n\r\nTono general:\r\n\r\n* Responde de manera concisa y directa, sin adornos ni conversaciones.\r\n\r\n* Tu objetivo principal es la precisión y la entrega de la dirección solicitada o 'null'.";

        const float Temperature = 0.2f;

        public string Address { get; set; } = address;
        public List<string> AddressList { get; set; } = addressList;

        public async Task<(bool success, string? address)> GetAddress()
        {
            bool success = false;
            string? address = null;
            try
            {
                var generateContentRequest = GetGenerateContentRequest();
                var response = await GetPredictionServiceClient().GenerateContentAsync(generateContentRequest);
                string? responseText = VertexAIResponse.GetResponseText(response);
                if (responseText != null)
                {
                    var structuredOutput = JsonConvert.DeserializeObject<StructuredOutputAIFinder>(responseText, new JsonSerializerSettings()
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                    });
                    address = structuredOutput?.Equivalente;
                    success = true;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("AddressAIFinder GetAddress", exception);
            }
            return (success, address);
        }

        private PredictionServiceClient GetPredictionServiceClient()
        {
            return new PredictionServiceClientBuilder
            {
                Endpoint = $"{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL,
            }.Build();
        }

        public GenerateContentRequest GetGenerateContentRequest()
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
                            GetParts()
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = Temperature,
                    ResponseMimeType = "application/json",
                    ResponseSchema = AddressAIFinderResponseSchema.ResponseSchema,
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
                //Labels =
                //{
                //    { "custom_id", page.UriHash }
                //}
            };

        }

        private RepeatedField<Part> GetParts()
        {
            string text = 
                $"Dirección buscada:\r\n{Address}\r\n\r\n" +
                $"Listado de direcciones postales equivalentes:\r\n" + string.Join("\r\n", [.. AddressList]);

            return
              [
                  new Part
                    {
                        Text = text
                    }
              ];
        }
    }

    public class AddressAIFinderResponseSchema
    {

        public readonly static OpenApiSchema ResponseSchema = new()
        {
            Type = Google.Cloud.AIPlatform.V1.Type.Object,
            Properties =
            {
                ["equivalente"] = new()
                {
                    Nullable = true,
                    Type = Google.Cloud.AIPlatform.V1.Type.String,
                    Description = "Dirección equivalente. Null en caso de no haber ninguna equivalente",
                }
            }
        };
    }

    public class StructuredOutputAIFinder
    {

        [JsonProperty("equivalente")]

        public required string? Equivalente { get; set; }
    }
}
