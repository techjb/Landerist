using landerist_library.Application.Listings;
using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class LegacyListingStore : IListingStore
{
    public Listing? Get(Page page, bool loadMedia, bool loadSources) =>
        global::landerist_library.Pages.Pages.GetListing(page, loadMedia, loadSources);

    public void Upsert(
        Page page,
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null) =>
        ES_Listings.InsertUpdate(page.Website, listing, unpublishDecision);
}
