using System.Text.Json.Serialization;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
#pragma warning disable CS8618
    public class VertexAIBatchResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("processed_time")]
        public string ProcessedTime { get; set; }

        [JsonPropertyName("request")]
        public Request Request { get; set; }

        [JsonPropertyName("response")]
        public VertexAIBatchResponseResponse Response { get; set; }
    }

    public class VertexAIBatchResponseResponse
    {
        [JsonPropertyName("candidates")]
        public List<VertexAIBatchResponseCandidate> Candidates { get; set; }
    }

    public class VertexAIBatchResponseCandidate
    {
        [JsonPropertyName("content")]
        public VertexAIBatchResponseContent Content { get; set; }
    }

    public class VertexAIBatchResponseContent
    {
        [JsonPropertyName("parts")]
        public List<VertexAIBatchResponsePart> Parts { get; set; }
    }

    public class VertexAIBatchResponsePart
    {
        [JsonPropertyName("text")]
        public string Text   { get; set; }
    }

#pragma warning restore CS8618
}
