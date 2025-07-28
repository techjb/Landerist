using landerist_library.Websites;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace landerist_library.Parse.ListingParser.LocalAI
{
    public class LocalAIRequest
    {

        private const string LOCALAI_URL = "http://192.168.1.29:1234";
        private const string LOCALAI_MODEL = "qwen/qwen3-30b-a3b";
        private const float TEMPERATURE = 0.2f;
        private const string SYSTEM_PROMPT = "Response siempre de forma ruda, maleducada y enfadado. Response siempre muy breve.";
        //private string SYSTEM_PROMPT = ParseListingSystem.GetSystemPrompt();
        public const int MAX_CONTEXT_WINDOW = 32768;



        public async Task<LocaAIResponse?> GetResponse(string text)
        {
            using HttpClient client = new();


            string json = $@"
            {{
                ""model"": ""{LOCALAI_MODEL}"",
                ""messages"": [
                    {{ ""role"": ""system"", ""content"": ""{SYSTEM_PROMPT}"" }},
                    {{ ""role"": ""user"", ""content"": ""{text}"" }}
                ],
                ""temperature"": {TEMPERATURE.ToString(CultureInfo.InvariantCulture)}
            }}";

            string url = LOCALAI_URL + "/v1/chat/completions";
            HttpContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync(url, httpContent);
                string result = await response.Content.ReadAsStringAsync();
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
    }
}
