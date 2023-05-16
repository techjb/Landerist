namespace landerist_library.Database
{
    public class DiscardedImages
    {
        private const string DISCARDED_IMAGES = "[DISCARDED_IMAGES]";

        public static bool Contains(Uri uri)
        {
            string query =
                "IF EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + DISCARDED_IMAGES + " " +
                "   WHERE Uri = @Uri) " +
                "SELECT 'true' " +
                "ELSE " +
                "SELECT 'false' ";

            return new DataBase().QueryBool(query, new Dictionary<string, object?> {
                {"Uri", uri.ToString() }
            });
        }

        public static bool Insert(Uri uri)
        {
            if (uri.ToString().Length > 400)
            {
                return false;
            }

            string query = 
                "INSERT INTO " + DISCARDED_IMAGES + " " +
                "VALUES (GETDATE(), @Uri)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Uri", uri.ToString() }
            });
        }
    }
}
