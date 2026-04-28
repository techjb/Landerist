using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using Newtonsoft.Json.Schema;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json.Nodes;


namespace landerist_library.Parse.ListingParser.OpenAI
{
    public class OpenAIRequest : SytemPrompt
    {
        public static readonly int MAX_CONTEXT_WINDOW = 128000;

        public static readonly string MODEL_NAME = "gpt-5-mini-2025-08-07";

        public static readonly double TEMPERATURE = 0f;

        private static readonly OpenAIClient OpenAIClient = new(PrivateConfig.OPENAI_API_KEY);

        public static readonly string? TOOL_CHOICE = "required";        

   

        public static ChatResponse? GetChatResponse(string userInput)
        {
            var messages = new List<Message>
            {
                new(Role.System, Text),
                new(Role.User, userInput),                
            };

            ChatRequest chatRequest = new(
                     messages: messages,
                     model: MODEL_NAME,
                     //temperature: TEMPERATURE,
                     responseFormat: TextResponseFormat.JsonSchema,
                     jsonSchema: OpenAIJsonSchema
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
                Logs.Log.WriteError("OpenAIRequest GetChatResponse", exception);
            }
            return null;
        }

        public static global::OpenAI.JsonSchema OpenAIJsonSchema
        {
            get
            {
                var schema = StructuredOutputSchema.GetJsonSchemaString();
                return new global::OpenAI.JsonSchema("esquema_de_respuesta", schema);
            }
        }
    }
}
