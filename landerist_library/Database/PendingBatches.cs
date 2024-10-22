using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Database
{
    public class PendingBatches
    {
        private const string PENDING_BATCHES = "[PENDING_BATCHES]";
        public static bool Insert(string id)
        {
            string query =
                "INSERT INTO " + PENDING_BATCHES + " " +
                "VALUES (GETDATE(), @Id)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Id", id }
            });
        }

        public static bool Delete(string id)
        {
            string query =
                "DELETE FROM " + PENDING_BATCHES + " " +
                "WHERE Id = @Id";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Id", id }
            });
        }

        public static List<string> Select()
        {
            string query =
                "SELECT Id " +
                "FROM " + PENDING_BATCHES + " " +
                "ORDER BY [Created] ASC";

            return new DataBase().QueryListString(query);        
        }
    }
}
