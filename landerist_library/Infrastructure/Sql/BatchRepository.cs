using landerist_library.Database;
using landerist_library.Parse.ListingParser;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class BatchRepository
    {
        private const string BatchesTable = "[BATCHES]";
        private readonly IDatabase _database;
        public BatchRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database;

        public bool Delete(string id)
        {
            string query =
                "DELETE FROM " + BatchesTable + " " +
                "WHERE Id = @Id";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Id", id }
            });
        }

        public List<Batch> Select(bool downloaded)
        {
            string query =
                "SELECT * " +
                "FROM " + BatchesTable + " " +
                "WHERE [Downloaded] = @Downloaded " +
                "ORDER BY [Created] ASC";

            DataTable dataTable = Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Downloaded", downloaded }
            });

            return Parse(dataTable);
        }

        public Batch? Select(string id)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + BatchesTable + " " +
                "WHERE [Id] = @Id";

            DataTable dataTable = Database.QueryTable(query, new Dictionary<string, object?>
            {
                { "Id", id }
            });

            return Parse(dataTable).FirstOrDefault();
        }

        public List<string> SelectAll(LLMProvider llmProvider)
        {
            string query =
                "SELECT Id " +
                "FROM " + BatchesTable + " " +
                "WHERE [LLMProvider] = @LLMProvider " +
                "ORDER BY [Created] ASC";

            return Database.QueryListString(query, new Dictionary<string, object?>
            {
                { "LLMProvider", llmProvider.ToString() }
            });
        }

        public bool Update(string id, bool downloaded)
        {
            string query =
                "UPDATE " + BatchesTable + " " +
                "SET [Downloaded] = @Downloaded " +
                "WHERE [Id] = @Id";

            return Database.Query(query, new Dictionary<string, object?>
            {
                { "Id", id },
                { "Downloaded", downloaded }
            });
        }

        private static List<Batch> Parse(DataTable dataTable)
        {
            List<Batch> batches = [];

            foreach (DataRow dataRow in dataTable.Rows)
            {
                batches.Add(new Batch
                {
                    Created = (DateTime)dataRow["Created"],
                    LLMProvider = Enum.Parse<LLMProvider>((string)dataRow["LLMProvider"]),
                    Id = (string)dataRow["Id"],
                    PagesUriHashes =
                    [
                        .. ((string)dataRow["PagesUriHashes"])
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    ],
                    Downloaded = (bool)dataRow["Downloaded"]
                });
            }

            return batches;
        }
    }
}
