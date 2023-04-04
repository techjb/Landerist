using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace landerist_library.Parse
{
    public class ChatGPT
    {

        public static readonly int MAX_REQUEST_LENGTH = 16000;

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
    }
}
