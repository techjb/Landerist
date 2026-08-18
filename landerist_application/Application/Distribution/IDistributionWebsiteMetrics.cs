using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Application.Distribution;

public interface IDistributionWebsiteMetrics
{
    int CountPages(Website website);
    int CountPagesScrapedSince(Website website, DateTime dateFrom);
    int CountPagesInsertedSince(Website website, DateTime dateFrom);
    int CountPagesParsedSince(Website website, DateTime dateFrom);
    int CountListings(Website website);
    int CountListingsSince(Website website, DateTime dateFrom);
    int CountPublishedListings(Website website);
    int CountPublishedListingsWithAddress(Website website);
    int CountPublishedListingsWithCoordinates(Website website);
    int CountPublishedListingsWithImages(Website website);
    int CountUnpublishedListings(Website website);
    int CountListings(
        ListingStatus status,
        Operation operation,
        PropertyType propertyType);
}
