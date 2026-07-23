namespace landerist_library.Database
{
    public class NotListingsCache
    {
        public const string TableName = "NOT_LISTINGS_CACHE";

        public static bool Insert(string host, string listingParserInputHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(listingParserInputHash) || !Configuration.Config.NOT_LISTING_CACHE_ENABLED)
            {
                return false;
            }

            string query =
                "INSERT INTO " + TableName + " " +
                "([Inserted], [Host], [ListingParserInputHash]) " +
                "VALUES (GETDATE(), @Host, @ListingParserInputHash)";

            return LegacyDatabase.Create().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ListingParserInputHash", listingParserInputHash }
            });
        }

        public static bool IsNotListing(string host, string listingParserInputHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(listingParserInputHash) || !Configuration.Config.NOT_LISTING_CACHE_ENABLED)
            {
                return false;
            }

            string query =
                "SELECT 1 " +
                "FROM " + TableName + " " +
                "WHERE [Host] = @Host AND " +
                "[ListingParserInputHash] = @ListingParserInputHash";
            return LegacyDatabase.Create().QueryExists(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ListingParserInputHash", listingParserInputHash }
            });
        }

        public static bool Delete(string host, string listingParserInputHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(listingParserInputHash))
            {
                return false;
            }

            string query =
                "DELETE FROM " + TableName + " " +
                "WHERE [Host] = @Host AND " +
                "[ListingParserInputHash] = @ListingParserInputHash";
            return LegacyDatabase.Create().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ListingParserInputHash", listingParserInputHash }
            });
        }

        public static bool DeleteAll()
        {
            string query =
                "DELETE FROM " + TableName;
            return LegacyDatabase.Create().Query(query);
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + TableName + " " +
                "WHERE [Inserted] < DATEADD(DAY, -30, GETDATE())";
            return LegacyDatabase.Create().Query(query);
        }
    }
}
