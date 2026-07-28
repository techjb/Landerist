using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Tasks;

public interface ILocalAiListingParser
{
    (PageType PageType, Listing? Listing, bool WaitingAiRequest) Parse(
        Page page,
        string userInput);
}
