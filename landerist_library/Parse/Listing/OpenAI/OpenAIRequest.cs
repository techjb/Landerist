using landerist_library.Configuration;
using landerist_library.Websites;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using OpenAI;
using OpenAI.Chat;


namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIRequest : ParseListingRequest
    {
        //public static readonly string SystemMessage =
        //   "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información:\r\n\r\n" +
        //   "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.).\r\n" +
        //   "2. Ubicación (puede ser la ciudad, barrio o dirección exacta).\r\n" +
        //   "3. Precio de venta o alquiler.\r\n" +
        //   "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.).\r\n\r\n" +
        //   "Evalúa el texto introducido por el usuario y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
        //   "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
        //   "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.\r\n\r\n" +
        //   "Response sólo con \"si\" o \"no\" en formato Json"
        //   ;       

        public static readonly int MAX_CONTEXT_WINDOW = 128000;

        public static readonly string MODEL_NAME =
            "gpt-4o-mini";
        //"gpt-4o";

        public static readonly double TEMPERATURE = 0;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        public static readonly string? TOOL_CHOICE = "required";

        public static bool TooManyTokens(Page page)
        {
            return TooManyTokens(page, MAX_CONTEXT_WINDOW);
        }

        public static ChatResponse? GetChatResponse(string userInput)
        {
            var messages = new List<Message>
            {
                new(Role.System, SystemPrompt),
                new(Role.User, userInput),
            };

            ChatRequest chatRequest;
            if (Config.STRUCTURED_OUTPUT)
            {
                chatRequest = new ChatRequest(
                    messages: messages,
                    model: MODEL_NAME,
                    temperature: TEMPERATURE,
                    responseFormat: ChatResponseFormat.JsonSchema,
                    jsonSchema: GetOpenAIJsonSchema()
                    );
            }
            else
            {
                chatRequest = new ChatRequest(
                    messages: messages,
                    model: MODEL_NAME,
                    temperature: TEMPERATURE,
                    tools: new OpenAITools().GetTools(),
                    toolChoice: TOOL_CHOICE,
                    responseFormat: ChatResponseFormat.Json
                );
            }

            try
            {
                DateTime dateStart = DateTime.Now;
                var response = Task.Run(async () => await OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest)).Result;
                Timers.Timer.SaveTimerOpenAI("OpenAIRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("OpenAIRequest GetChatResponse", exception);
            }
            return null;
        }

        public static global::OpenAI.JsonSchema GetOpenAIJsonSchema()
        {
            var schema = GetSchema();
            return new global::OpenAI.JsonSchema("OpenAIStructuredOutput", schema);
        }

        public static string GetSchema()
        {
            JSchemaGenerator generator = new()
            {
                DefaultRequired = Required.AllowNull,
                GenerationProviders =
                {
                    new StringEnumGenerationProvider(),
                }
            };

            JSchema jSChema = generator.Generate(typeof(OpenAIStructuredOutput));
            SetAdditionalPropertiesFalse(jSChema);
            return jSChema.ToString();
        }

        private static void SetAdditionalPropertiesFalse(JSchema schema)
        {
            if ((schema.Type & JSchemaType.Object) == JSchemaType.Object)
            {
                schema.AllowAdditionalProperties = false;
            }

            foreach (var propertySchema in schema.Properties.Values)
            {
                SetAdditionalPropertiesFalse(propertySchema);
            }

            if (schema.Items != null)
            {
                foreach (var itemSchema in schema.Items)
                {
                    SetAdditionalPropertiesFalse(itemSchema);
                }
            }

            if (schema.AnyOf != null)
            {
                foreach (var subschema in schema.AnyOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (schema.AllOf != null)
            {
                foreach (var subschema in schema.AllOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (schema.OneOf != null)
            {
                foreach (var subschema in schema.OneOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }
        }
    }
}
