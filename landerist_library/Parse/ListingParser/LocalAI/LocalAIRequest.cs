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
        private const string SERVER_PORT = "8000";
        private const float TEMPERATURE = 0.0f;
        public const int MAX_CONTEXT_WINDOW = 65536;

        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromMinutes(10)
        };

        private readonly string Url;

        public LocalAIRequest()
        {
            string ip = "localhost";

            if (Config.IsConfigurationLocal())
            {
                try
                {
                    var hostEntry = Dns.GetHostEntry(PrivateConfig.MACHINE_NAME_LANDERIST_03);
                    ip = hostEntry.AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?
                        .ToString() ?? "localhost";
                }
                catch (Exception exception)
                {
                    Logs.Log.WriteError("LocalAIRequest ctor - DNS resolution", exception);
                    ip = "localhost";
                }
            }

            Url = $"http://{ip}:{SERVER_PORT}/v1/chat/completions";
        }

        public async Task<LocaAIResponse?> GetResponse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            try
            {
                var requestBody = GetRequestBody(text);
                string json = JsonSerializer.Serialize(requestBody);
                using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                DateTime dateStart = DateTime.Now;
                HttpResponseMessage response = await HttpClient.PostAsync(Url, httpContent);
                string result = await response.Content.ReadAsStringAsync();
                Timers.Timer.SaveTimerLocalAI("LocalAIRequest", dateStart);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"LocalAIRequest GetResponse error: {response.StatusCode} - {result}");
                }

                return JsonSerializer.Deserialize<LocaAIResponse>(
                    result,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LocalAIRequest GetResponse", exception);
                return null;
            }
        }

        private object GetRequestBody(string text)
        {
            return new
            {
                temperature = TEMPERATURE,
                //max_completion_tokens = 3000,
                top_p = 1.0,
                top_k = -1,
                messages = new[]
                {
                    new { role = "system", content = GetExtendedSystemPrompt() },
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
            return SytemPrompt.Text + " " +
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
