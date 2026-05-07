using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        public static Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE [listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT [PageType], COUNT(*) " +
                "FROM " + PAGES + " " +
                where + " " +
                "GROUP BY [PageType] " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null)
        {
            string where = listingStatus != null
                ? "WHERE [listingStatus] = @listingStatus "
                : string.Empty;
            string query =
                "SELECT CONVERT(VARCHAR,  [HttpStatusCode], 23), COUNT(*) " +
                "FROM " + PAGES + " " +
                where + " " +
                "GROUP BY CONVERT(VARCHAR,  [HttpStatusCode], 23) " +
                "ORDER BY COUNT(*) DESC";

            return new DataBase().QueryDictionary(query, new Dictionary<string, object?>
            {
                { "listingStatus", listingStatus.ToString() }
            });
        }

        public static Dictionary<string, object?> GroupByNextUpdate()
        {
            string query =
                "SELECT  CONVERT(VARCHAR, [NextUpdate], 23) AS [DateWhithoutTime], COUNT(*) AS [Total] " +
                "FROM " + PAGES + " " +
                "GROUP BY CONVERT(VARCHAR, [NextUpdate], 23) " +
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
