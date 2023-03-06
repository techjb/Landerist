using System.Data;

namespace landerist_library.Websites
{
    public class Websites
    {
        protected const string TABLE_WEBSITES = "[WEBSITES]";

        public static List<Website> GetAll()
        {
            var dataTable = GetDataTableAll();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeNotOk()
        {
            var dataTable = GetDataTableStatusCodeNotOk();
            return ParseWebsites(dataTable);
        }

        private static DataTable GetDataTableStatusCodeNotOk()
        {
            string query = 
                "SELECT * " +
                "FROM " + TABLE_WEBSITES  + " " +
                "WHERE [StatusCode] <> 200 ";
            return new Database().QueryTable(query);
        }
        
        private static DataTable GetDataTableAll()
        {
            string query = "SELECT * FROM " + TABLE_WEBSITES;
            return new Database().QueryTable(query);
        }

        private static List<Website> ParseWebsites(DataTable dataTable)
        {
            var list = new List<Website>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new (dataRow);
                list.Add(website);
            }
            return  list;
        }
    }
}
