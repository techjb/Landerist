using OpenAI;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{

#pragma warning disable CS8618
#pragma warning disable IDE1006

    public class RequestData
    {
        public string custom_id { get; set; }
        public string method { get; set; }
        public string url { get; set; }
    }
    public class NonStructuredRequestData : RequestData
    {
        public NonStructuredBody body { get; set; }
    }

    public class StructuredRequestData : RequestData
    {
        public StructuredBody body { get; set; }
    }

    public class Body
    {
        public string model { get; set; }
        public List<BatchMessage> messages { get; set; }
        public double temperature { get; set; }
    }

    public class NonStructuredBody : Body
    {
        public string? tool_choice { get; set; }
        public List<Tool> tools { get; set; }
        public NonStructuredResponseFormat response_format { get; set; }
    }

    public class StructuredBody : Body
    {
        public StructuredResponseFormat response_format { get; set; }
    }

    public class BatchMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class NonStructuredResponseFormat
    {
        public string? type { get; set; }
    }

    public class StructuredResponseFormat
    {
        public string? type { get; set; }

        public JsonSchema json_schema { get; set; }
    }


#pragma warning restore CS8618
#pragma warning restore IDE1006
}
