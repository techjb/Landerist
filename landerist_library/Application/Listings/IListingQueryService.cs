using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingQueryService
{
    Listing? Get(Page page, bool loadMedia, bool loadSources);
    IReadOnlyCollection<Listing> GetUnpublishedBefore(DateTime unlistingDate);
}

public interface IListingMaintenanceService
{
    bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    bool Delete(string guid);
    bool DeleteAll();
}
