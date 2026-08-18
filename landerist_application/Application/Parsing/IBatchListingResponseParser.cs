using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Parsing;

public interface IBatchListingResponseParser
{
    (PageType PageType, Listing? Listing) Parse(
        Page page,
        string? response,
        BatchProvider provider);
}
