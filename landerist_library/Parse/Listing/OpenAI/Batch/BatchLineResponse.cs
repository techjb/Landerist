using OpenAI.Chat;
using System.Text.Json.Serialization;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{
#pragma warning disable CS8618

    public class BatchLineResponse
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
        //public BatchResponseBody Body { get; set; }
        public ChatResponse Body { get; set; }
    }

    //public class BatchResponseBody
    //{
    //    [JsonPropertyName("id")]
    //    public string Id { get; set; }

    //    [JsonPropertyName("object")]
    //    public string Object { get; set; }

    //    [JsonPropertyName("created")]
    //    public long Created { get; set; }

    //    [JsonPropertyName("model")]
    //    public string Model { get; set; }

    //    [JsonPropertyName("choices")]
    //    public List<Choice> Choices { get; set; }

    //    [JsonPropertyName("usage")]
    //    public Usage Usage { get; set; }

    //    [JsonPropertyName("system_fingerprint")]
    //    public string SystemFingerprint { get; set; }
    //}

    //public class Choice
    //{
    //    [JsonPropertyName("index")]
    //    public int Index { get; set; }

    //    [JsonPropertyName("message")]
    //    public BatchResponseMessage Message { get; set; }

    //    [JsonPropertyName("logprobs")]
    //    public object Logprobs { get; set; }

    //    [JsonPropertyName("finish_reason")]
    //    public string FinishReason { get; set; }
    //}

    //public class BatchResponseMessage
    //{
    //    [JsonPropertyName("role")]
    //    public string Role { get; set; }

    //    [JsonPropertyName("content")]
    //    public string Content { get; set; }

    //    [JsonPropertyName("refusal")]
    //    public object Refusal { get; set; }
    //}

    //public class Usage
    //{
    //    [JsonPropertyName("prompt_tokens")]
    //    public int PromptTokens { get; set; }

    //    [JsonPropertyName("completion_tokens")]
    //    public int CompletionTokens { get; set; }

    //    [JsonPropertyName("total_tokens")]
    //    public int TotalTokens { get; set; }

    //    [JsonPropertyName("prompt_tokens_details")]
    //    public TokenDetails PromptTokensDetails { get; set; }

    //    [JsonPropertyName("completion_tokens_details")]
    //    public TokenDetails CompletionTokensDetails { get; set; }
    //}

    //public class TokenDetails
    //{
    //    [JsonPropertyName("cached_tokens")]
    //    public int CachedTokens { get; set; }

    //    [JsonPropertyName("reasoning_tokens")]
    //    public int ReasoningTokens { get; set; }
    //}


#pragma warning restore CS8618
}
