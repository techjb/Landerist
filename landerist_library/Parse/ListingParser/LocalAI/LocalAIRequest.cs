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

        private const string SERVER_PORT = "1234";
        private const string MODEL_NAME = "qwen/qwen3-30b-a3b-2507";
        //private const string MODEL_NAME = "qwen/qwen3-4b-2507";
        private const float TEMPERATURE = 0.1f;
        public const int MAX_CONTEXT_WINDOW = 65536;
        private readonly string Url;


        public LocalAIRequest()
        {
            var hostEntry = Dns.GetHostEntry(PrivateConfig.MACHINE_NAME_LANDERIST_03);
            string ip = hostEntry.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork).ToString();
            Url = $"http://{ip}:{SERVER_PORT}/v1/chat/completions";
        }

        public async Task<LocaAIResponse?> GetResponse(string text)
        {

            var requestBody = new
            {
                model = MODEL_NAME,
                temperature = TEMPERATURE,
                max_tokens = -1,
                //top_p= 0.8f,
                enable_thinking = false,
                //stream = false,
                messages = new[]
                {
                    //new { role = "system", content = ParseListingSystem.GetExtendedSystemPrompt() },
                    new { role = "system", content = GetExtendedSystemPrompt() },
                    new { role = "user", content = text }
                },
                format = "json",
                //response_format = new
                //{
                //    type = "json_schema",
                //    json_schema = OpenAIRequest.GetOpenAIJsonSchema2()                    
                //}
            };

            string json = JsonSerializer.Serialize(requestBody);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                DateTime dateStart = DateTime.Now;
                using HttpClient client = new();
                HttpResponseMessage response = await client.PostAsync(Url, httpContent);
                string result = await response.Content.ReadAsStringAsync();
                Timers.Timer.SaveTimerOpenAI("LocalAIRequest", dateStart);
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<LocaAIResponse>(result);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LocalAIRequest GetResponse", exception);
            }
            return null;
        }

        private static string GetExtendedSystemPrompt()
        {
            return ParseListingSystem.SystemPrompt + " " +
                "Responde SIEMPRE y ÚNICAMENTE con un objeto JSON. No añadas texto antes ni después del JSON. " +
                "El objeto JSON debe tener la siguiente estructura exacta: " + OpenAIRequest.GetOpenAIJsonSchema() + " " +
                "Si no encuentras algún dato, usa 'null'. No incluyas texto adicional fuera del JSON.";
        }

        public static void PrintOutputSchema()
        {
            var schema = StructuredOutputSchema.GetJsonSchema();
            Console.WriteLine(schema);
        }
    }
}
