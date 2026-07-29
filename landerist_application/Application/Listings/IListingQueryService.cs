using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingQueryService
{
    Listing? Get(Page page, bool loadMedia, bool loadSources);
    Task<Listing?> GetAsync(
        Page page,
        bool loadMedia,
        bool loadSources,
        CancellationToken cancellationToken = default);
    IReadOnlyCollection<Listing> GetUnpublishedBefore(DateTime unlistingDate);
}

public interface IListingMaintenanceService
{
    bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    Task<bool> UpdateAsync(Listing listing, ListingUnpublishDecision? unpublishDecision = null, CancellationToken cancellationToken = default);
    bool Delete(string guid);
    Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default);
    bool DeleteAll();
    Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default);
}
