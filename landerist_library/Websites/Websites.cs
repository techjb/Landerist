using System.Data;

namespace landerist_library.Websites
{
    public class Websites
    {
        protected const string TABLE_WEBSITES = "[WEBSITES]";

        public static List<Website> GetAll()
        {
            var dataTable = GetDataTable();
            return ParseWebsites(dataTable);
        }

        private static DataTable GetDataTable()
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
