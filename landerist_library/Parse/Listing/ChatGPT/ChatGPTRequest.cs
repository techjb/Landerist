using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using OpenAI.Chat;
using OpenAI;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTRequest(string systemMessage, List<Tool> tools)
    {

        //https://platform.openai.com/docs/models/overview  
        public static readonly int MAX_CONTEXT_WINDOW = 
            16385; // gpt-3.5-turbo-0125
            //128000; // gpt-4-turbo        
        
        private static readonly string Model =
            "gpt-3.5-turbo-0125";
            //"gpt-4-turbo";

        private readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);
        private readonly string SystemMessage = systemMessage;
        private readonly List<Tool>? Tools = tools;
        private readonly string? ToolChoice = "auto";

        public ChatGPTRequest(string systemMessage, Tool tool) : this(systemMessage, [tool])
        {
            ToolChoice = tool.Function.Name;
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
                model: Model,
                temperature: 0,
                tools: Tools,
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
                Logs.Log.WriteLogErrors("ChatGPT_Request", exception);
            }
            return null;
        }

        protected static bool TooManyTokens(string systemMessage, string userMessage)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(systemMessage).Count;
            int userTokens = GPT3Tokenizer.Encode(userMessage).Count;

            int totalTokens = systemTokens + userTokens;
            return totalTokens > MAX_CONTEXT_WINDOW;
        }

        public void ListModels()
        {
            var models = Task.Run(async () => await OpenAIClient.ModelsEndpoint.GetModelsAsync()).Result;
            List<string> list = [];
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
