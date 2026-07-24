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
    bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null);
    bool Delete(string guid);
    bool DeleteAll();
    bool UpdateAddress(
        string guid,
        double? latitude,
        double? longitude,
        bool? locationIsAccurate,
        string? locationResolver = null);
}

public interface IListingMediaRepository
{
    void Insert(Listing listing);
    bool Delete(string guid);
    bool DeleteAll();
    DataTable GetMedia(Listing listing);
}

public interface IListingSourceRepository
{
    void Insert(Listing listing);
    bool Delete(string guid);
    bool DeleteAll();
    DataTable GetSources(Listing listing);
    DataTable GetListingsWithoutSourcePages();
}
