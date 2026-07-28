using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingLocationEnricher
{
    void EnrichLocation(Page page, Listing listing);
}