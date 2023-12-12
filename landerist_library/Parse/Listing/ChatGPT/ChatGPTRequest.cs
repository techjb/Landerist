using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using OpenAI.Chat;
using OpenAI;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTRequest(string systemMessage, List<Tool> tools)
    {

        //https://platform.openai.com/docs/models/overview  
        // gpt-3.5-turbo-1106: 16,385 
        // gpt-4-1106-preview: 128,000
        public static readonly int MAX_TOKENS = 16385;
        public static readonly string Model_GPT_4_1106_Preview = "gpt-4-1106-preview";
        public static readonly string Model_GPT_3_5_Turbo_16k = "gpt-3.5-turbo-16k";


        private readonly OpenAIClient OpenAIClient = new(Config.OPENAI_API_KEY);
        private readonly string SystemMessage = systemMessage;
        private readonly List<Tool>? Tools = tools;
        private readonly string? ToolChoice = "auto";

        public ChatGPTRequest(string systemMessage, Tool tool) : this(systemMessage, [tool])
        {
            ToolChoice = tool.Function.Name;
        }

        protected ChatResponse? GetResponse(string? userInput, bool modelGPT_4)
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

            var model = modelGPT_4 ? Model_GPT_4_1106_Preview : Model_GPT_3_5_Turbo_16k;
            var chatRequest = new ChatRequest(
                messages: messages,
                model: model,
                temperature: 0,
                tools: Tools,
                toolChoice: ToolChoice
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
