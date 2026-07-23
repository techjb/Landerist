using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingStore
{
    Listing? Get(Page page, bool loadMedia, bool loadSources);

    void Upsert(Page page, Listing listing, ListingUnpublishDecision? unpublishDecision = null);
}
