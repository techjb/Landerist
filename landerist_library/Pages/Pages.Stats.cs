using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        public static Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE L.[listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT P.[PageType], COUNT(*) " +
                "FROM " + PAGES + " AS P " +
                "LEFT JOIN " + ES_Listings.TABLE_ES_LISTINGS + " AS L ON L.[guid] = P.[UriHash] " +
                where + " " +
                "GROUP BY P.[PageType] " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE L.[listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT CONVERT(VARCHAR, P.[HttpStatusCode], 23), COUNT(*) " +
                "FROM " + PAGES + " AS P " +
                "LEFT JOIN " + ES_Listings.TABLE_ES_LISTINGS + " AS L ON L.[guid] = P.[UriHash] " +
                where + " " +
                "GROUP BY CONVERT(VARCHAR, P.[HttpStatusCode], 23) " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByNextScrape()
        {
            string query =
                "SELECT  CONVERT(VARCHAR, [NextScrape], 23) AS [DateWhithoutTime], COUNT(*) AS [Total] " +
                "FROM " + PAGES + " " +
                "GROUP BY CONVERT(VARCHAR, [NextScrape], 23) " +
                "ORDER BY [DateWhithoutTime] ASC";

            return new DataBase().QueryDictionary(query);
        }

        public static Dictionary<string, object?> CountByHttpStatusCode()
        {
            string query =
                "SELECT CAST([HttpStatusCode] AS VARCHAR), COUNT(*) " +
                "FROM " + PAGES + " " +
                "GROUP BY [HttpStatusCode] " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query);
        }
    }
}
