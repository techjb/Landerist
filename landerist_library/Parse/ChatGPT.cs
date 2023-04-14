using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;

namespace landerist_library.Parse
{
    public class ChatGPT
    {

        // GPT-3.5-Turbo: 4096
        // GPT-4-8K: 8192
        // GPT-4-32K: 32768
        public static readonly int MAX_TOKENS = 4096;

        private const string SystemMessage =
            "Eres un servidor de información que únicamente responde en formato JSON";

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

        public string? GetResponse(string userInput)
        {
            userInput = ParseUserInput(userInput);
            Conversation.AppendUserInput(userInput);
            try
            {

                string response = Task.Run(async () => await Conversation.GetResponseFromChatbotAsync()).Result; ;
                return response;
            }
            catch
            {
                return null;
            }
        }

        public static string ParseUserInput(string userIput)
        {
            return
                "Dado el siguiente texto:\n" +
                "----\n" +
                userIput + "\n" +
                "----\n" +
                "Proporciona una representación JSON que siga estrictamente este esquema:\n" +
                ListingResponseSchema.GetSchema() + "\n" +
                "Cuando no encuentras alguno de los campos, escribe null";
        }

        public static bool IsRequestAllowed(string request)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemMessage).Count;

            request = ParseUserInput(request);
            int userTokens = GPT3Tokenizer.Encode(request).Count;

            int totalTokens = systemTokens + userTokens;
            return totalTokens < MAX_TOKENS;
        }
    }
}
