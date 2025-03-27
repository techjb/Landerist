namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{

#pragma warning disable CS8618
#pragma warning disable IDE1006


    public class VertexAIBatchRequest
    {
        public Request request { get; set; }
    }

    public class Request
    {
        public List<Content> contents { get; set; }
        public SystemInstruction system_instruction { get; set; }

        public GenerationConfig generation_config { get; set; }

        public Dictionary<string, string> labels { get; set; }
    }

    public class SystemInstruction
    {
        public List<Part> parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }


    public class Content
    {
        public string role { get; set; }
        public List<Part> parts { get; set; }
    }

    public class GenerationConfig
    {
        public float temperature { get; set; }

        public string response_mime_type { get; set; }

        public object response_schema { get; set; }
    }


#pragma warning restore CS8618
#pragma warning restore IDE1006
}
