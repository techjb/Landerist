using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingStore
{
    Listing? Get(Page page, bool loadMedia, bool loadSources);

    Task<Listing?> GetAsync(
        Page page,
        bool loadMedia,
        bool loadSources,
        CancellationToken cancellationToken = default);

    void Upsert(Page page, Listing listing, ListingUnpublishDecision? unpublishDecision = null);

    Task UpsertAsync(
        Page page,
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null,
        CancellationToken cancellationToken = default);
}
