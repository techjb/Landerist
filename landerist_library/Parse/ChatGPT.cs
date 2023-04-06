using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using AI.Dev.OpenAI.GPT;

namespace landerist_library.Parse
{
    public class ChatGPT
    {

        // GPT-3.5-Turbo: 4096
        // GPT-4-8K: 8192
        // GPT-4-32K: 32768
        public static readonly int MAX_TOKENS = 4096;

        //public static readonly int MAX_TEXT_LENGTH = 16000;

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
                Temperature = 0,
                //MaxTokens = 50,
            };
            Conversation = openAIAPI.Chat.CreateConversation(chatRequest);
            Conversation.AppendSystemMessage(SystemMessage);
        }

        public async Task<string?> GetResponse(string userInput)
        {
            Conversation.AppendUserInput(userInput);
            try
            {
                return await Conversation.GetResponseFromChatbotAsync();
            }
            catch
            {
                return null;
            }
        }

        public static bool IsRequestAllowed(string request)
        {
            //https://github.com/dluc/openai-tools
            int userTokens = GPT3Tokenizer.Encode(request).Count;
            string systemMessage = GetSystemMessage();
            int systemTokens = GPT3Tokenizer.Encode(systemMessage).Count;
            int totalTokens = userTokens + systemTokens;
            return totalTokens < MAX_TOKENS;
        }

        public static string GetSystemMessage()
        {
            return SystemMessage  + Environment.NewLine + ListingResponseSchema.GetSchema();
        }
    }
}
