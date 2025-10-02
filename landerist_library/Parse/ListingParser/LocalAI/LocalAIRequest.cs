using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace landerist_library.Parse.ListingParser.LocalAI
{
    public class LocalAIRequest
    {

        private const string SERVER_PORT =
            //"1234"
            "8000"
            ;

        private const float TEMPERATURE = 0.1f;
        public const int MAX_CONTEXT_WINDOW = 65536;
        private readonly string Url;


        public LocalAIRequest()
        {
            string ip = "localhost";
            if (Config.IsConfigurationLocal())
            {
                var hostEntry = Dns.GetHostEntry(PrivateConfig.MACHINE_NAME_LANDERIST_03);
                ip = hostEntry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
            }
            Url = $"http://{ip}:{SERVER_PORT}/v1/chat/completions";
        }

        public async Task<LocaAIResponse?> GetResponse(string text)
        {
            var requestBody = GetRequestBody(text);
            string json = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                DateTime dateStart = DateTime.Now;
                using HttpClient client = new();
                client.Timeout = TimeSpan.FromMinutes(3);
                HttpResponseMessage response = await client.PostAsync(Url, httpContent);
                string result = await response.Content.ReadAsStringAsync();
                Timers.Timer.SaveTimerLocalAI("LocalAIRequest", dateStart);
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<LocaAIResponse>(result);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message + " Tokens: " + GPT3Tokenizer.Encode(text).Count);
                Logs.Log.WriteError("LocalAIRequest GetResponse", exception);
            }
            return null;
        }

        private object GetRequestBody(string text)
        {
            // LMStudio
            //return new
            //{
            //    model = "qwen/qwen3-30b-a3b-2507",
            //    temperature = TEMPERATURE,
            //    max_tokens = -1,
            //    //top_p= 0.8f,
            //    enable_thinking = false,
            //    //stream = false,
            //    messages = new[]
            //    {
            //        //new { role = "system", content = ParseListingSystem.GetExtendedSystemPrompt() },
            //        new { role = "system", content = GetExtendedSystemPrompt() },
            //        new { role = "user", content = text }
            //    },
            //    format = "json",
            //    //response_format = new
            //    //{
            //    //    type = "json_schema",
            //    //    json_schema = OpenAIRequest.GetOpenAIJsonSchema2()                    
            //    //}
            //};

            return new
            {
                temperature = TEMPERATURE,
                messages = new[]
                {
                    new { role = "system", content = ParseListingSystem.GetSystemPrompt() },
                    //new { role = "system", content = GetExtendedSystemPrompt() },
                    new { role = "user", content = text }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = OpenAIRequest.OpenAIJsonSchema
                }
            };
        }

        private static string GetExtendedSystemPrompt()
        {
            return ParseListingSystem.SystemPrompt + " " +
                "Responde SIEMPRE y ÚNICAMENTE con un objeto JSON. No añadas texto antes ni después del JSON. " +
                "El objeto JSON debe tener la siguiente estructura exacta: " + OpenAIRequest.OpenAIJsonSchema + " " +
                "Si no encuentras algún dato, usa 'null'. No incluyas texto adicional fuera del JSON.";
        }

        public static void PrintOutputSchema()
        {
            var schema = StructuredOutputSchema.GetJsonSchemaString();
            Console.WriteLine(schema);
        }
    }
}
