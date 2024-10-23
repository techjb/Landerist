using landerist_library.Configuration;
using landerist_library.Websites;
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

        public static readonly string MODEL_NAME = "gpt-4o-mini";

        public static readonly double TEMPERATURE = 0;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        public static readonly string? TOOL_CHOICE = "required";        

        public static bool TooManyTokens(Page page)
        {
            return TooManyTokens(page, MAX_CONTEXT_WINDOW);
        }

        public static ChatResponse? GetResponse(string userInput)
        {
            var messages = new List<Message>
            {
                new(Role.System, SystemPrompt),
                new(Role.User, userInput),
            };

            var tools = new OpenAITools().GetTools();

            var chatRequest = new ChatRequest(
                messages: messages,
                model: MODEL_NAME,
                temperature: TEMPERATURE,
                tools: tools,
                toolChoice: TOOL_CHOICE,
                responseFormat:  ChatResponseFormat.Json                
                );
            
            try
            {
                DateTime dateStart = DateTime.Now;
                var response = Task.Run(async () => await OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest)).Result;
                Timers.Timer.SaveTimerOpenAI("OpenAIRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("OpenAIRequest GetResponse", exception);
            }
            return null;
        }    
    }
}
