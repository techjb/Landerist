using landerist_library.Configuration;
using landerist_library.Parse.ListingParser;
using System.Data;

namespace landerist_library.Database
{
    public class Batches
    {
        private const string BATCHES = "[BATCHES]";

        public static bool Insert(string id)
        {
            string query =
                "INSERT INTO " + BATCHES + " " +
                "VALUES (GETDATE(), @LLMProvider, @Id, 0)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"LLMProvider", Config.LLM_PROVIDER.ToString() },
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

        public static List<Batch> SelectNonDownloaded()
        {
            return Select(false);
        }

        public static List<Batch> SelectDownloaded()
        {
            return Select(true);
        }

        private static List<Batch> Select(bool downloaded)
        {
            string query =
                "SELECT * " +
                "FROM " + BATCHES + " " +
                "WHERE [Downloaded] = @Downloaded " +
                "ORDER BY [Created] ASC";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                {"Downloaded", downloaded }
            });
            return Parse(dataTable);
        }

        public static Batch? Select(string id)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + BATCHES + " " +
                "WHERE [Id] = @Id";
            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                {"Id", id }
            });
            List<Batch> batches = Parse(dataTable);
            return batches.Count > 0 ? batches[0] : null;
        }

        private static List<Batch> Parse(DataTable dataTable)
        {
            List<Batch> batches = [];
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var batch = new Batch()
                {
                    Created = (DateTime)dataRow["Created"],
                    LLMProvider = (LLMProvider)Enum.Parse(typeof(LLMProvider), (string)dataRow["LLMProvider"]),
                    Id = (string)dataRow["Id"],
                    Downloaded = (bool)dataRow["Downloaded"]
                };
                batches.Add(batch);
            }
            return batches;
        }

        public static List<string> SelectAll(LLMProvider lLMProvider)
        {
            string query =
                "SELECT Id " +
                "FROM " + BATCHES + " " +
                "WHERE [LLMProvider] = @LLMProvider" +
                "ORDER BY [Created] ASC";

            return new DataBase().QueryListString(query, new Dictionary<string, object?>()
            {
                { "LLMProvider", lLMProvider.ToString() }
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
