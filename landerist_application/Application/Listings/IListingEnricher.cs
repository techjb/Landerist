using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingEnricher
{
    void Enrich(Page page, Listing listing);
}
