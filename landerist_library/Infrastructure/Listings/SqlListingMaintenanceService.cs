using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingMaintenanceService : IListingMaintenanceService
{
    private readonly ListingRepository _listings;
    private readonly MediaRepository _media;
    private readonly SourceRepository _sources;

    public SqlListingMaintenanceService(
        ListingRepository listings,
        MediaRepository media,
        SourceRepository sources)
    {
        ArgumentNullException.ThrowIfNull(listings);
        ArgumentNullException.ThrowIfNull(media);
        ArgumentNullException.ThrowIfNull(sources);
        _listings = listings;
        _media = media;
        _sources = sources;
    }

    public bool Update(
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null)
    {
        ArgumentNullException.ThrowIfNull(listing);
        return _listings.Update(listing, unpublishDecision);
    }

    public bool Delete(string guid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        return _listings.Delete(guid) &&
            _media.Delete(guid) &&
            _sources.Delete(guid);
    }

    public bool DeleteAll()
    {
        bool listingsDeleted = _listings.Delete();
        bool mediaDeleted = _media.Delete();
        bool sourcesDeleted = _sources.Delete();
        return listingsDeleted && mediaDeleted && sourcesDeleted;
    }
}
