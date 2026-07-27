namespace landerist_library.Parse.ListingParser;

public sealed record ListingParserOrchestrationOptions(
    bool BatchEnabled,
    LLMProvider Provider);
