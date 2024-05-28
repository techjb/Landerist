using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using landerist_library.Websites;
using OpenAI;
using OpenAI.Chat;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTRequest
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

        public static readonly string SystemPrompt =
            "Tu tarea consiste en procesar el texto proporcionado por el usuario, identificando si corresponde a un anuncio inmobiliario. " +
            "De ser así, deberás analizar meticulosamente el contenido para determinar que efectivamente se trata de un único anuncio y proceder a extraer los datos relevantes.  " +
            "Estos deberán ser presentados en un formato estructurado JSON, asegurando una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
            "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";


        //public static readonly string SystemMessage =
        //    "Tu tarea consiste en procesar el código html proporcionado por el usuario, identificando si corresponde a un anuncio inmobiliario. " +
        //    "De ser así, deberás analizar meticulosamente el contenido para determinar que efectivamente se trata de un único anuncio y proceder a extraer los datos relevantes.  " +
        //    "Estos deberán ser presentados en un formato estructurado JSON, asegurando una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
        //    "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

        public static readonly int MAX_CONTEXT_WINDOW =
                 16385; // gpt-3.5-turbo-0125
                        //128000; // gpt-4o     

        public static readonly string ModelName =
                "gpt-3.5-turbo-0125";
        //"gpt-4o";

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);
        private static readonly string? ToolChoice = "auto";
      
        public static bool TooManyTokens(Page page)
        {
            if (page.ResponseBodyText == null)
            {
                return false;
            }

            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemPrompt).Count;
            string? text = UserTextInput.GetText(page);
            if(text == null)
            {
                return false;
            }
            int userTokens = GPT3Tokenizer.Encode(text).Count;

            int totalTokens = systemTokens + userTokens;
            return totalTokens > MAX_CONTEXT_WINDOW;
        }

        public static ChatResponse? GetResponse(string userInput)
        {
            var messages = new List<Message>
            {
                new(Role.System, SystemPrompt),
                new(Role.User, userInput),
            };

            var tools = ChatGPTTools.GetTools();

            var chatRequest = new ChatRequest(
                messages: messages,
                model: ModelName,
                temperature: 0,
                tools: tools,
                toolChoice: ToolChoice,
                responseFormat: ChatResponseFormat.Json
                );

            try
            {
                DateTime dateStart = DateTime.Now;
                var response = Task.Run(async () => await OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest)).Result;
                Timers.Timer.SaveTimerChatGPT("ChatGPTRequest", dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ChatGPTRequest", exception);
            }
            return null;
        }

        public static (string?, string?) GetFunctionNameAndArguments(ChatResponse chatResponse)
        {
            try
            {
                return (chatResponse.FirstChoice.Message.ToolCalls[0].Function.Name, 
                    chatResponse.FirstChoice.Message.ToolCalls[0].Function.Arguments.ToString());
            }
            catch
            {
            }
            return (null, null);
        }
    }
}
