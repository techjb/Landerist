using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class _ChatGPTRequest
    {

        //https://platform.openai.com/docs/models/overview  
        //gpt-3.5-turbo: 4096
        // gpt-3.5-turbo-16k: 16384
        // GPT-4-8K: 8192
        // GPT-4-32K: 32768
        //public static readonly int MAX_TOKENS = 8192;
        public static readonly int MAX_TOKENS = 4096;

        private readonly Conversation Conversation;

        public _ChatGPTRequest(string systemMessage)
        {
            OpenAIAPI openAIAPI = new(Config.OPENAI_API_KEY);
            var chatRequest = new ChatRequest()
            {
                Model = Model.ChatGPTTurbo,
                //Model = Model.GPT4,
                Temperature = 0,                
            };

            Conversation = openAIAPI.Chat.CreateConversation(chatRequest);
            Conversation.AppendSystemMessage(systemMessage);
        }

        protected string? GetResponse(string? userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return null;
            }
            Conversation.AppendUserInput(userInput);
            try
            {
                DateTime dateStart = DateTime.Now;
                string response = Task.Run(async () => await Conversation.GetResponseFromChatbotAsync()).Result;
                Timers.Timer.SaveTimerChatGPT(string.Empty, dateStart);
                return response;
            }
            catch(Exception exception) 
            {
                Console.WriteLine(exception.Message.ToString());
                return null;
            }
        }

        protected static bool IsLengthAllowed(string systemMessage, string userMessage)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(systemMessage).Count;
            int userTokens = GPT3Tokenizer.Encode(userMessage).Count;

            int totalTokens = systemTokens + userTokens;
            return totalTokens < MAX_TOKENS;
        }
    }
}
