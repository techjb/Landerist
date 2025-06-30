using landerist_library.Parse.ListingParser;


namespace landerist_library.Database
{
    public class Batch
    {
        public required DateTime Created { get; set; }

        public required LLMProvider LLMProvider { get; set; }

        public required string Id { get; set; }

        public HashSet<string> PagesUriHashes { get; set; } = [];

        public required bool Downloaded { get; set; }
    }
}
