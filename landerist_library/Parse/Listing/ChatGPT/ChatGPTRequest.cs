using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using OpenAI.Chat;
using OpenAI;
using OpenAI.Models;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTRequest
    {

        //https://platform.openai.com/docs/models/overview  
        //gpt-3.5-turbo: 4096
        // gpt-3.5-turbo-16k: 16384
        // GPT-4-8K: 8192
        // GPT-4-32K: 32768
        //public static readonly int MAX_TOKENS = 8192;
        public static readonly int MAX_TOKENS = 4096;

        private readonly OpenAIClient OpenAIClient = new (Config.OPENAI_API_KEY);
        private readonly string SystemMessage;
        private readonly List<Tool>? Tools;
        private readonly string? ToolChoice;

        public ChatGPTRequest(string systemMessage = "")
        {
            SystemMessage = systemMessage;            
        }

        public ChatGPTRequest(string systemMessage, List<Tool> tools, string toolChoice = "auto") : this(systemMessage)
        {
            Tools = tools;           
            ToolChoice = toolChoice;
        }

        protected ChatResponse? GetResponse(string? userInput)
        {
            if (string.IsNullOrEmpty(userInput))
            {
                return null;
            }
            var messages = new List<Message>
            {
                new(Role.System, SystemMessage),
                new(Role.User, userInput),
            };

            var chatRequest = new ChatRequest(
                messages: messages,                
                model: "gpt-4-1106-preview",
                //responseFormat: ChatResponseFormat.Json,
                temperature: 0,
                tools: Tools,
                toolChoice: ToolChoice
                );

            //var chatRequest = new ChatRequest(
            //    messages: messages,
            //    functions: Functions,
            //    functionCall: FunctionCall,
            //    model: Model.GPT3_5_Turbo_16K,                
            //    temperature: 0
            //    );

            try
            {
                DateTime dateStart = DateTime.Now;
                var response = Task.Run(async () => await OpenAIClient.ChatEndpoint.GetCompletionAsync(chatRequest)).Result;
                Timers.Timer.SaveTimerChatGPT(string.Empty, dateStart);
                return response;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ChatGPT_Request", exception);
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

        public void ListModels()
        {
            var models = Task.Run(async () => await OpenAIClient.ModelsEndpoint.GetModelsAsync()).Result;
            List<string> list = new ();
            foreach (var model in models)
            {
                list.Add(model.ToString());
            }
            list.Sort();
            foreach (var model in list)
            {
                Console.WriteLine(model.ToString());    
            }
        }       
    }
}
