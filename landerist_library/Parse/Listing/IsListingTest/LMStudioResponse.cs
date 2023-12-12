namespace landerist_library.Parse.Listing.IsListingTest
{
    public class Message
    {
        public required string Role { get; set; } 
        public required string Content { get; set; }
    }

    public class Choice
    {
        public int Index { get; set; }
        public required Message Message { get; set; }
        public required string FinishReason { get; set; }
    }

    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class LMStudioResponse
    {
        public required string Id { get; set; }
        public required string Object { get; set; }
        public long Created { get; set; }
        public required string Model { get; set; }
        public required List<Choice> Choices { get; set; }
        public required Usage Usage { get; set; }
    }
}
