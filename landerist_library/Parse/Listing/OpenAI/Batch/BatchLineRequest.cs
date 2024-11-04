using OpenAI;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{

#pragma warning disable CS8618
#pragma warning disable IDE1006

    public class RequestData
    {
        public string custom_id { get; set; }
        public string method { get; set; }
        public string url { get; set; }
        public Body body { get; set; }
    }

    public class Body
    {
        public string model { get; set; }
        public List<BatchMessage> messages { get; set; }

        public double temperature { get; set; }

        public string? tool_choice { get; set; }

        public ResponseFormat response_format { get; set; }

        public List<Tool> tools { get; set; }
    }

    public class BatchMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class ResponseFormat
    {
        public string? type { get; set; }
    }


#pragma warning restore CS8618
#pragma warning restore IDE1006
}
