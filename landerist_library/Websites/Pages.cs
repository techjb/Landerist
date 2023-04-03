using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        public const string TABLE_PAGES = "[PAGES]";

        public Pages()
        {

        }

        public List<Page> GetAll()
        {
            Console.WriteLine("Reading all pages");
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " ";

            DataTable dataTable = new Database().QueryTable(query);
            return GetPages(dataTable);
        }

        public List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Domain] = @Domain";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object?> {
                {"Domain", website.Host }
            });

            return GetPages(dataTable);
        }

        private List<Page> GetPages(DataTable dataTable)
        {
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(dataRow);
                pages.Add(page);
            }
            return pages;
        }

        public static bool Remove(Website website)
        {
            string query =
               "DELETE FROM " + TABLE_PAGES + " " +
               "WHERE [Host] = @Host";

            return new Database().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
        }

        public static void Insert(List<Uri> uris)
        {
            foreach (var uri in uris)
            {
                var page = new Page(uri);
                page.Insert();
            }
        }
    }
}
