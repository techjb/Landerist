using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Sql
{
    public class WebsitePageMetricsRepository
    {
        private static readonly HashSet<string> SupportedDateColumns =
        [
            "LastScrape",
            "Inserted",
            "LastParseListing"
        ];

        public int CountPages(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryInt(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        public int CountPagesSince(string host, string dateColumn, DateTime dateFrom)
        {
            if (!SupportedDateColumns.Contains(dateColumn))
            {
                throw new ArgumentException("Unexpected date column.", nameof(dateColumn));
            }

            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host " +
                $"AND [{dateColumn}] >= @DateFrom";

            return new DataBase().QueryInt(query, new Dictionary<string, object?> {
                {"Host", host },
                {"DateFrom", dateFrom }
            });
        }

        public bool HasPageTypeListing(string host)
        {
            string query =
                "SELECT 1 " +
                "FROM " + Pages.Pages.PAGES + " " +
                "WHERE [Host] = @Host AND [PageType] = @PageType";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                { "Host", host },
                { "PageType", PageType.Listing.ToString() }
            });
        }

        public bool HasPublishedListings(string host)
        {
            string query =
                "SELECT 1 " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host AND [listingStatus] = @ListingStatus";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                { "Host", host },
                { "ListingStatus", ListingStatus.published.ToString() }
            });
        }
    }
}
