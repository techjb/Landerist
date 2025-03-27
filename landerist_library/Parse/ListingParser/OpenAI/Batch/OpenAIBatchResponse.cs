using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
#pragma warning disable CS8618

    public class OpenAIBatchResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("custom_id")]
        public string CustomId { get; set; }

        [JsonPropertyName("response")]
        public Response Response { get; set; }

        [JsonPropertyName("error")]
        public object Error { get; set; }
    }

    public class Response
    {
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("request_id")]
        public string RequestId { get; set; }

        [JsonPropertyName("body")]
        public ChatResponse Body { get; set; }
    }


#pragma warning restore CS8618
}
