using landerist_library.Database;
using landerist_library.Parse.PageType;
using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        public const string TABLE_PAGES = "[PAGES]";

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

        public static List<Page> GetPagesUnknowPageType(Website website)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [Host] = @Host AND " +
                "[PageType] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", website.Host },                
            });

            return GetPages(website, dataTable);
        }

        public static List<Page> GetPages(PageType pageType)
        {
            string query =
                "SELECT " + TABLE_PAGES + ".*, " + Websites.TABLE_WEBSITES + ".* " +
                "FROM " + TABLE_PAGES + " " +
                "INNER JOIN " + Websites.TABLE_WEBSITES + " ON " + TABLE_PAGES + ".[Host] = " + Websites.TABLE_WEBSITES + ".[Host] " +
                "WHERE [PageType] = @PageType";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });

            return GetPages(dataTable);
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

        public static List<Page> GetUnknowPageType(int? rows = null)
        {
            string topRows = string.Empty;
            string orderBy = string.Empty;
            if (rows != null)
            {
                topRows = "TOP " + rows + " ";
                orderBy = "ORDER BY NEWID()";
            }
            string query =
                "SELECT " + topRows + TABLE_PAGES + ".*, " + Websites.TABLE_WEBSITES + ".* " +
                "FROM " + TABLE_PAGES + " " +
                "INNER JOIN " + Websites.TABLE_WEBSITES + " ON " + TABLE_PAGES + ".[Host] = " + Websites.TABLE_WEBSITES + ".[Host] " +
                "WHERE [PageType] IS NULL " +
                orderBy;

            Console.WriteLine("Reading non scraped ..");
            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                "SELECT " + TABLE_PAGES + ".*, " + Websites.TABLE_WEBSITES + ".* " +
                "FROM " + TABLE_PAGES + " " +
                "INNER JOIN " + Websites.TABLE_WEBSITES + " ON " + TABLE_PAGES + ".[Host] = " + Websites.TABLE_WEBSITES + ".[Host] " +
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
            Console.WriteLine("Parsing to pages ..");
            List<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new(dataRow);
                Page page = new(website, dataRow);
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

            bool sucess = new DataBase().Query(query, new Dictionary<string, object?> {
                {"Host", website.Host }
            });
            if (sucess)
            {
                website.SetNumPagesToZero();
            }
            return sucess;
        }

        public static bool DeleteAll()
        {
            string query =
               "DELETE FROM " + TABLE_PAGES;

            return new DataBase().Query(query);
        }

        public static void Insert(Website website, Uri uri)
        {
            var page = new Page(website, uri);
            page.Insert();
        }

        public static DataTable GetIsListingResponseBodyText(int? top = null)
        {
            string queryTop = string.Empty;
            if (top != null)
            {
                queryTop = " TOP " + top;
            }
            string query =
                "SELECT " + queryTop + " [IsListing], [ResponseBodyText] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [IsListing] IS NOT NULL";

            return new DataBase().QueryTable(query);
        }

        public static DataTable GetUriResponseBodyText(int? top = null)
        {
            string queryTop = string.Empty;
            if (top != null)
            {
                queryTop = " TOP " + top;
            }
            string query =
                "SELECT " + queryTop + " [Uri], [ResponseBodyText] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [ResponseBodyText] IS NOT NULL";

            return new DataBase().QueryTable(query);
        }

        public static DataTable GetIsListingResponseBodyText(int top, bool isListing, bool random)
        {
            string query =
                "SELECT  TOP " + top + " [IsListing], [ResponseBodyText] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [IsListing] = @IsListing";

            if (random)
            {
                query += " ORDER BY NEWID()";
            }

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                {"IsListing", isListing }
            });
        }

        public static List<string> GetUris(bool isListing)
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE IsListing = @IsListing";
            return new DataBase().QueryListString(query, new Dictionary<string, object?>()
            {
                { "IsListing", isListing }
            });
        }

        public static List<string> GetUris()
        {
            string query =
                "SELECT [Uri] " +
                "FROM " + TABLE_PAGES;
            return new DataBase().QueryListString(query);
        }
    }
}
