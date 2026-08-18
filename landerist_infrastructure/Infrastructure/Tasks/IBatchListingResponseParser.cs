using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Tasks;

public interface IBatchListingResponseParser
{
    (PageType PageType, Listing? Listing) Parse(
        Page page,
        string? response,
        BatchProvider provider);
}
