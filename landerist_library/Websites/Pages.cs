using landerist_library.Database;
using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        protected const string TABLE_PAGES = "[PAGES]";

        public Pages()
        {

        }

        public static DataTable GetAll()
        {
            Console.WriteLine("Reading all pages");
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " ";

            return new DataBase().QueryTable(query);
        }

        public static List<Page> GetPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetNonScrapedPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[Updated] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetNonScraped()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Updated] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [HttpStatusCode] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public static List<Page> GetUnknowIsListingPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[IsListing] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }


        public static List<Page> GetIsNotListingPages(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[IsListing] = 0";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });

            return GetPages(website, dataTable);
        }

        private static List<Page> GetPages(DataTable dataTable)
        {
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(dataRow);
                pages.Add(page);
            }
            return pages;
        }

        private static List<Page> GetPages(Website website, DataTable dataTable)
        {
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Page page = new(website, dataRow);
                pages.Add(page);
            }
            return pages;
        }

        public static bool Delete(Website website)
        {
            string query =
               "DELETE FROM " + TABLE_PAGES + " " +
               "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
        }

        public static void Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            page.Insert();
        }
    }
}
