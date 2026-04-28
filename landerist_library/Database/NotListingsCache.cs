namespace landerist_library.Database
{
    public class NotListingsCache
    {
        public const string TableName = "NOT_LISTINGS_CACHE";

        public static bool Insert(string host, string listingParserInputHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(listingParserInputHash))
            {
                return false;
            }

            string query =
                "INSERT INTO " + TableName + " " +
                "([Inserted], [Host], [ListingParserInputHash]) " +
                "VALUES (GETDATE(), @Host, @ListingParserInputHash)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ListingParserInputHash", listingParserInputHash }
            });
        }

        public static bool IsNotListing(string host, string listingParserInputHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(listingParserInputHash))
            {
                return false;
            }

            string query =
                "SELECT 1 " +
                "FROM " + TableName + " " +
                "WHERE [Host] = @Host AND " +
                "[ListingParserInputHash] = @ListingParserInputHash";
            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
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
            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ListingParserInputHash", listingParserInputHash }
            });
        }

        public static bool DeleteAll()
        {
            string query =
                "DELETE FROM " + TableName;
            return new DataBase().Query(query);
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + TableName + " " +
                "WHERE [Inserted] < DATEADD(DAY, -30, GETDATE())";
            return new DataBase().Query(query);
        }
    }
}
