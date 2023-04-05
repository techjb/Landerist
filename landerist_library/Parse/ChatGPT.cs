using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using AI.Dev.OpenAI.GPT;

namespace landerist_library.Parse
{
    public class ChatGPT
    {

        // Para GPT-3.5-Turbo es 4096
        // Para GPT-4-8K es 8192
        // Para GPT-4-32K es 32768
        public static readonly int MAX_TOKENS = 4096;

        public static readonly int MAX_TEXT_LENGTH = 16000;

        private const string SystemMessage =
            "Eres un clasificador de textos. Si el texto introducido contiene los datos de venta o alquiler " +
            "de un sólo inmueble, con su tipología, precio y localización, responde \"si\". " +
            "Responde \"no\" en caso contrario. Si no lo sabes responde \"null\". Solo responde con una palabra.";

        private readonly Conversation Conversation;

        public ChatGPT()
        {
            OpenAIAPI openAIAPI = new(Config.OPENAI_API_KEY);
            var chatRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                //MaxTokens = 50,
            };
            Conversation = openAIAPI.Chat.CreateConversation(chatRequest);
            Conversation.AppendSystemMessage(SystemMessage);
        }

        public async Task<string?> GetListing(string ResponseBodyText)
        {
            Conversation.AppendUserInput(ResponseBodyText);
            try
            {
                return await Conversation.GetResponseFromChatbotAsync();
            }
            catch
            {
                return null;
            }
        }

        public static bool IsTextAllowed(string text)
        {
            // más simple, pero peor.
            //return text.Length < MAX_TEXT_LENGTH;

            //https://github.com/dluc/openai-tools
            return GPT3Tokenizer.Encode(text).Count < MAX_TOKENS;            
        }
    }
}
