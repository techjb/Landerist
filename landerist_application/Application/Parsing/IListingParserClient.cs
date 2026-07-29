using landerist_library.Parsing;
using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public sealed record ListingParserClientResult(
    string? ResponseText,
    bool WaitingAIRequest,
    string? Diagnostic = null);

public interface IListingParserClient
{
    LLMProvider Provider { get; }

    ListingParserClientResult GetResponse(
        Page page,
        string userInput);
}
