namespace landerist_library.Database
{
    public class NotListingsCache
    {
        public const string TableName = "NOT_LISTINGS_CACHE";

        public static bool Insert(string host, string responseBodyTextHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(responseBodyTextHash))
            {
                return false;
            }

            string query =
                "INSERT INTO " + TableName + " " +
                "([Inserted], [Host], [ResponseBodyTextHash]) " +
                "VALUES (GETDATE(), @Host, @ResponseBodyTextHash)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ResponseBodyTextHash", responseBodyTextHash }
            });
        }

        public static bool IsNotListing(string host, string responseBodyTextHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(responseBodyTextHash))
            {
                return false;
            }

            string query =
                "SELECT 1 " +
                "FROM " + TableName + " " +
                "WHERE [Host] = @Host AND " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash";
            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ResponseBodyTextHash", responseBodyTextHash }
            });
        }

        public static bool Delete(string host, string responseBodyTextHash)
        {
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(responseBodyTextHash))
            {
                return false;
            }

            string query =
                "DELETE FROM " + TableName + " " +
                "WHERE [Host] = @Host AND " +
                "[ResponseBodyTextHash] = @ResponseBodyTextHash";
            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", host },
                {"ResponseBodyTextHash", responseBodyTextHash }
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
