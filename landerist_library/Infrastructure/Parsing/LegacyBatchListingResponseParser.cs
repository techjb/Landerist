using landerist_library.Parsing;
using landerist_library.Application.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;
using landerist_library.Infrastructure.Tasks;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyBatchListingResponseParser(ParseListing parser)
    : IBatchListingResponseParser
{
    public (PageType PageType, Listing? Listing) Parse(
        Page page,
        string? response,
        BatchProvider provider) =>
        parser.ParseResponse(
            page,
            response,
            provider switch
            {
                BatchProvider.OpenAI => LLMProvider.OpenAI,
                BatchProvider.VertexAI => LLMProvider.VertexAI,
                _ => throw new ArgumentOutOfRangeException(nameof(provider))
            });
}
