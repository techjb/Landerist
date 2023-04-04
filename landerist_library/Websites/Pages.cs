using landerist_library.Database;
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

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Domain] = @Domain";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Domain", website.Host }
            });

            return GetPages(website, dataTable);
        }

        private List<Page> GetPages(Website website, DataTable dataTable)
        {
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(website, dataRow);
                pages.Add(page);
            }
            return pages;
        }

        public static bool Remove(Website website)
        {
            string query =
               "DELETE FROM " + TABLE_PAGES + " " +
               "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
        }

        public static void Insert(Website website, List<Uri> uris)
        {
            foreach (var uri in uris)
            {
                var page = new Page(website, uri);
                page.Insert();
            }
        }
    }
}
