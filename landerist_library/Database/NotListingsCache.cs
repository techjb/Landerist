namespace landerist_library.Database
{
    public class NotListingsCache
    {
        public const string TableName = "NOT_LISTINGS_CACHE";


        public static bool Insert(string responseBodyTextHash)
        {
            string query =
                "INSERT INTO " + TableName + " " +
                "VALUES (GETDATE(), @ResponseBodyTextHash)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"ResponseBodyTextHash", responseBodyTextHash }
            });
        }

        public static bool IsNotListing(string responseBodyTextHash)
        {
            if (string.IsNullOrEmpty(responseBodyTextHash))
            {
                return false;
            }
            string query =
                "SELECT 1 " +
                "FROM " + TableName + " " +
                "WHERE ResponseBodyTextHash = @ResponseBodyTextHash";
            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"ResponseBodyTextHash", responseBodyTextHash }
            });
        }

        public static bool Delete(string responseBodyTextHash)
        {
            string query =
                "DELETE FROM " + TableName + " " +
                "WHERE ResponseBodyTextHash = @ResponseBodyTextHash";
            return new DataBase().Query(query, new Dictionary<string, object?> {
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
                "WHERE [Inserted] < DATEADD(DAY, -90, GETDATE())";
            return new DataBase().Query(query);
        }
    }
}
