using landerist_library.Parsing;
using landerist_library.Application.Parsing;
namespace landerist_library.Application.Parsing;

public sealed record ListingParserOrchestrationOptions(
    bool BatchEnabled,
    LLMProvider Provider);
