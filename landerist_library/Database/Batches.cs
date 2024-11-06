namespace landerist_library.Database
{
    public class Batches
    {
        private const string BATCHES = "[BATCHES]";
        public static bool Insert(string id)
        {
            string query =
                "INSERT INTO " + BATCHES + " " +
                "VALUES (GETDATE(), @Id, 0)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Id", id }
            });
        }

        public static bool Delete(string id)
        {
            string query =
                "DELETE FROM " + BATCHES + " " +
                "WHERE Id = @Id";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Id", id }
            });
        }

        public static List<string> SelectNonDownloaded()
        {
            return Select(false);
        }

        public static List<string> SelectDownloaded()
        {
            return Select(true);
        }

        private static List<string> Select(bool downloaded)
        {
            string query =
                "SELECT Id " +
                "FROM " + BATCHES + " " +
                "WHERE [Downloaded] = @Downloaded " +
                "ORDER BY [Created] ASC";

            return new DataBase().QueryListString(query, new Dictionary<string, object?>()
            {
                {"Downloaded", downloaded }
            });
        }

        public static bool UpdateToDownloaded(string id)
        {
            return Update(id, true);
        }

        private static bool Update(string id, bool downloaded)
        {
            string query =
                "UPDATE " + BATCHES + " " +
                "SET [Downloaded] = @Downloaded " +
                "WHERE [Id] = @Id";


            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Id", id },
                {"Downloaded", downloaded }
            });
        }
    }
}
