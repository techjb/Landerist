using System.Text.Json.Serialization;

namespace landerist_library.Parse.ListingParser.LocalAI
{
#pragma warning disable CS8618 
    public class LocaAIResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }


        [JsonPropertyName("object")]
        public string ObjectType { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }

        [JsonPropertyName("stats")]
        public Dictionary<string, object> Stats { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; }


        public string? GetResponseText()
        {
            if (Choices != null && Choices.Count > 0)
            {
                return Choices[0].Message.Content;
            }            
            return null;
        }

        public string? GetFinishReason()
        {
            if (Choices != null && Choices.Count > 0)
            {
                return Choices[0].FinishReason;                
            }
            return null;
        }   

        public string GetStats()
        {
            if (Stats != null && Stats.Count > 0)
            {
                return string.Join(", ", Stats.Select(kv => kv.Key + ": " + kv.Value));
            }
            return string.Empty;
        }   
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("logprobs")]
        public object Logprobs { get; set; }

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("reasoning_content")]
        public string? ReasoningContent { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
#pragma warning restore CS8618
}
