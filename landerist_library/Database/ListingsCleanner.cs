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

        public static int UnpublishListingsByPageType(string host, Pages.PageType pageType)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "host", host },
                { "pageType", pageType.ToString() },
                { "published", ListingStatus.published.ToString() },
                { "unpublished", ListingStatus.unpublished.ToString() },
            };

            string where =
                "L.[listingStatus] = @published AND " +
                "L.[Host] = @host AND " +
                "EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + Pages.Pages.PAGES + " AS P " +
                "   WHERE P.[UriHash] = L.[guid] AND " +
                "   P.[Host] = @host AND " +
                "   P.[PageType] = @pageType" +
                ")";

            int listingsToUnpublish = new DataBase().QueryInt(
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE " + where,
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
                "WHERE " + where,
                updateParameters);

            if (!success)
            {
                return 0;
            }

            success = new DataBase().Query(
                "UPDATE P " +
                "SET P.[ListingStatus] = @unpublished " +
                "FROM " + Pages.Pages.PAGES + " AS P " +
                "WHERE P.[Host] = @host AND " +
                "P.[PageType] = @pageType AND " +
                "EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "   WHERE L.[guid] = P.[UriHash] AND " +
                "   L.[Host] = @host AND " +
                "   L.[listingStatus] = @unpublished" +
                ")",
                updateParameters);

            return success ? listingsToUnpublish : 0;
        }
    }
}
