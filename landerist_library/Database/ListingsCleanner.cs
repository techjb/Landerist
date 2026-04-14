using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Database
{
    public class ListingsCleanner
    {
        public static int UnpublishListingsWithoutPage()
        {
            var parameters = new Dictionary<string, object?>
            {
                { "published", ListingStatus.published.ToString() },
                { "unpublished", ListingStatus.unpublished.ToString() },
            };

            int listingsToUnpublish = new DataBase().QueryInt(
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE L.[listingStatus] = @published AND " +
                "NOT EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + Pages.Pages.PAGES + " AS P " +
                "   WHERE P.[UriHash] = L.[guid]" +
                ")",
                parameters);

            if (listingsToUnpublish == 0)
            {
                return 0;
            }

            var updateParameters = new Dictionary<string, object?>(parameters)
            {
                { "updated", DateTime.Now },
                { "unlistingDate", DateTime.Now },
            };

            bool success = new DataBase().Query(
                "UPDATE L " +
                "SET " +
                "   L.[listingStatus] = @unpublished, " +
                "   L.[updated] = @updated, " +
                "   L.[unlistingDate] = @unlistingDate " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE L.[listingStatus] = @published AND " +
                "NOT EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + Pages.Pages.PAGES + " AS P " +
                "   WHERE P.[UriHash] = L.[guid]" +
                ")",
                updateParameters);

            return success ? listingsToUnpublish : 0;
        }
    }
}
