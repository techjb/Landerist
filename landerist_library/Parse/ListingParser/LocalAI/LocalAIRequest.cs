using landerist_library.Configuration;
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
        private const string MODEL_NAME = "cyankiwi/Qwen3.6-35B-A3B-AWQ-4bit";
        private const float TEMPERATURE = 0.0f;
        private const int SERVER_MAX_MODEL_LEN = 60000;
        private const int MAX_COMPLETION_TOKENS = 12000;
        public const int MAX_CONTEXT_WINDOW = SERVER_MAX_MODEL_LEN - MAX_COMPLETION_TOKENS;

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
                    var hostEntry = Dns.GetHostEntry(AppConfig.MACHINE_NAME_LANDERIST_03);
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
            var jsonSchema = GetJsonSchema();

            return new
            {
                model = MODEL_NAME,
                temperature = TEMPERATURE,
                max_tokens = MAX_COMPLETION_TOKENS,
                top_p = 1.0,
                top_k = -1,
                chat_template_kwargs = new
                {
                    enable_thinking = false
                },
                messages = new[]
                {
                    new { role = "system", content = GetExtendedSystemPrompt() },
                    new { role = "user", content = NormalizeUserInput(text) }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "esquema_de_respuesta",
                        schema = jsonSchema,
                        strict = true
                    }
                },
                //structured_outputs = new
                //{
                //    json = jsonSchema
                //},
                //guided_json = jsonSchema
            };
        }

        private static JsonElement GetJsonSchema()
        {
            using var jsonDocument = JsonDocument.Parse(StructuredOutputSchema.GetJsonSchemaString());
            return jsonDocument.RootElement.Clone();
        }

        private static string NormalizeUserInput(string text)
        {
            return text.Replace('"', '”');
        }

        private static string GetExtendedSystemPrompt()
        {
            return SystemPrompt.Text + " " +
                "Responde SIEMPRE y ÚNICAMENTE con un objeto JSON válido. No añadas texto antes ni después del JSON. " +
                "En todos los valores string, escapa siempre los caracteres especiales de JSON: las comillas dobles interiores deben ir precedidas por una barra invertida, y también deben escaparse barras invertidas, saltos de línea y tabuladores. " +
                "Nunca incluyas comillas dobles literales sin escapar dentro de un valor string. " +
                "Si el input contiene marcadores de imagen como LANDERIST_IMAGE_A1B2C3D4E5F60708, úsalos exactamente como valor de url_de_la_imagen cuando correspondan a imágenes del anuncio; no intentes reconstruir, expandir ni inventar la URL original. " +
                "El objeto JSON debe tener la siguiente estructura exacta: " + StructuredOutputSchema.GetJsonSchemaString() + " " +
                "Si no encuentras algún dato, usa 'null'. No incluyas texto adicional fuera del JSON.";
        }

        public static void PrintOutputSchema()
        {
            var schema = StructuredOutputSchema.GetJsonSchemaString();
            Console.WriteLine(schema);
        }
    }
}
