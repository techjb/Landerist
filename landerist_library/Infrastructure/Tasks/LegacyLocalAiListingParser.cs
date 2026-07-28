using landerist_library.Application.Statistics;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyLocalAiListingParser(
    ParseListing parser,
    HostStatistics hostStatistics) : ILocalAiListingParser
{
    public (PageType PageType, Listing? Listing, bool WaitingAiRequest) Parse(
        Page page,
        string userInput) =>
        parser.ParseLocalAI(page, userInput, hostStatistics);
}
