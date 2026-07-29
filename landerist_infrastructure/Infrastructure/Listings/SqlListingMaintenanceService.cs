using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Listings;

public sealed class SqlListingMaintenanceService : IListingMaintenanceService
{
    private readonly IListingRecordRepository _listings;
    private readonly IListingMediaRepository _media;
    private readonly IListingSourceRepository _sources;

    public SqlListingMaintenanceService(
        ListingRepository listings,
        IListingMediaRepository media,
        IListingSourceRepository sources)
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

    public Task<bool> UpdateAsync(
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(listing);
        return _listings.UpdateAsync(listing, unpublishDecision, cancellationToken);
    }

    public bool Delete(string guid)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        return _listings.Delete(guid) &&
            _media.Delete(guid) &&
            _sources.Delete(guid);
    }

    public async Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(guid);
        return await _listings.DeleteAsync(guid, cancellationToken).ConfigureAwait(false) &&
            await _media.DeleteAsync(guid, cancellationToken).ConfigureAwait(false) &&
            await _sources.DeleteAsync(guid, cancellationToken).ConfigureAwait(false);
    }

    public bool DeleteAll()
    {
        bool listingsDeleted = _listings.DeleteAll();
        bool mediaDeleted = _media.DeleteAll();
        bool sourcesDeleted = _sources.DeleteAll();
        return listingsDeleted && mediaDeleted && sourcesDeleted;
    }

    public async Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        bool listingsDeleted = await _listings.DeleteAllAsync(cancellationToken).ConfigureAwait(false);
        bool mediaDeleted = await _media.DeleteAllAsync(cancellationToken).ConfigureAwait(false);
        bool sourcesDeleted = await _sources.DeleteAllAsync(cancellationToken).ConfigureAwait(false);
        return listingsDeleted && mediaDeleted && sourcesDeleted;
    }
}
