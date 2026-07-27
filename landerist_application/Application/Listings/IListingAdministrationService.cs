using landerist_library.Pages;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingAdministrationService
{
    void Upsert(Website website, Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    bool UpdateAddress(string guid, double? latitude, double? longitude, bool? locationIsAccurate, string? locationResolver = null);
    Listing? Get(Page page, bool loadMedia, bool loadSources);
    Listing? Get(string guid, bool loadMedia = false, bool loadSources = false);
    SortedSet<Listing> GetAll(bool loadMedia, bool loadSources);
    SortedSet<Listing> GetByStatus(ListingStatus listingStatus, bool loadMedia = true, bool loadSources = true);
    SortedSet<Listing> GetByStatus(
        ListingStatus listingStatus,
        Operation operation,
        PropertyType propertyType,
        bool loadMedia,
        bool loadSources);
    SortedSet<Listing> GetByHost(string host, ListingStatus? listingStatus = null);
    SortedSet<Listing> GetByDateRange(bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo);
    SortedSet<Listing> GetByDateRange(
        ListingStatus listingStatus,
        bool loadMedia,
        bool loadSources,
        DateOnly dateFrom,
        DateOnly dateTo);
    SortedSet<Listing> GetWithCadastralReference();
    SortedSet<Listing> GetWithoutLauName();
    SortedSet<Listing> GetWithCadastralReferenceAndNoAddress();
    SortedSet<Listing> GetWithoutCadastralReferenceAndAccurateLocation();
    SortedSet<Listing> GetAccurateLocationWithoutCadastralReference();
    bool Delete(string guid);
    bool DeleteAll();
    void RepairListingsWithoutSources();
}
