using landerist_library.Pages;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Application.Listings;

public interface IListingRecordRepository
{
    bool Insert(
        Listing listing,
        string host,
        ListingUnpublishDecision? unpublishDecision,
        out Exception? exception);
    Task<bool> InsertAsync(
        Listing listing,
        string host,
        ListingUnpublishDecision? unpublishDecision,
        CancellationToken cancellationToken = default);
    bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    Task<bool> UpdateAsync(
        Listing listing,
        ListingUnpublishDecision? unpublishDecision = null,
        CancellationToken cancellationToken = default);
    bool Delete(string guid);
    Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default);
    bool DeleteAll();
    Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default);
    bool UpdateAddress(
        string guid,
        double? latitude,
        double? longitude,
        bool? locationIsAccurate,
        string? locationResolver = null);
    Task<bool> UpdateAddressAsync(
        string guid,
        double? latitude,
        double? longitude,
        bool? locationIsAccurate,
        string? locationResolver = null,
        CancellationToken cancellationToken = default);
}

public interface IListingMediaRepository
{
    void Insert(Listing listing);
    Task InsertAsync(Listing listing, CancellationToken cancellationToken = default);
    bool Delete(string guid);
    Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default);
    bool DeleteAll();
    Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default);
    DataTable GetMedia(Listing listing);
    Task<DataTable> GetMediaAsync(
        Listing listing,
        CancellationToken cancellationToken = default);
}

public interface IListingSourceRepository
{
    void Insert(Listing listing);
    Task InsertAsync(Listing listing, CancellationToken cancellationToken = default);
    bool Delete(string guid);
    Task<bool> DeleteAsync(string guid, CancellationToken cancellationToken = default);
    bool DeleteAll();
    Task<bool> DeleteAllAsync(CancellationToken cancellationToken = default);
    DataTable GetSources(Listing listing);
    Task<DataTable> GetSourcesAsync(
        Listing listing,
        CancellationToken cancellationToken = default);
    DataTable GetListingsWithoutSourcePages();
}
