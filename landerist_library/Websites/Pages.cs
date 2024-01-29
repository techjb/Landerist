using landerist_library.Database;
using System.Data;

namespace landerist_library.Websites
{
    public class Pages
    {
        public const string TABLE_PAGES = "[PAGES]";

        public static List<Page> GetPages()
        {
            Console.WriteLine("Reading all pages");
            string query = QueryPages();

            var dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
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

        public static List<Page> GetPages(PageType pageType)
        {
            string query =
                QueryPages() +
                "WHERE [PageType] = @PageType";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() }
            });

            return GetPages(dataTable);
        }

        public static List<Page> GetPages(PageType pageType, int daysLastUpdate, int? pageTypeCounterMultiplier = null)
        {
            DateTime now = DateTime.Now;
            DateTime updated = now.AddDays(-daysLastUpdate);

            string query =
                QueryPages() +
                "WHERE [PageType] = @PageType AND [Updated] < @Updated ";

            if (pageTypeCounterMultiplier != null)
            {
                query += "AND [Updated] < DATEADD(DAY, -(PageTypeCounter * @PageTypeCounterMultiplier), @Now)";
            }

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"PageType", pageType.ToString() },
                {"Updated", updated },
                {"Now", now },
                {"PageTypeCounterMultiplier", pageTypeCounterMultiplier }
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

        public static List<Page> GetUnknowPageType(Website website)
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

        public static List<Page> GetUnknownPageType(int? rows = null)
        {
            string orderBy = string.Empty;
            if (rows != null)
            {
                orderBy = "ORDER BY NEWID()";
            }
            string query =
                QueryPages(rows) +
                "WHERE [PageType] IS NULL " +
                orderBy;

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }


        public static List<Page> GetUnknowHttpStatusCode()
        {
            string query =
                QueryPages() +
                "WHERE [HttpStatusCode] IS NULL";

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetPages(dataTable);
        }

        private static string QueryPages(int? rows = null)
        {
            string topRows = string.Empty;
            if (rows != null)
            {
                topRows = "TOP " + rows + " ";
            }

            return
                "SELECT " + topRows + TABLE_PAGES + ".*, " + Websites.TABLE_WEBSITES + ".* " +
                "FROM " + TABLE_PAGES + " " +
                "INNER JOIN " + Websites.TABLE_WEBSITES + " ON " + TABLE_PAGES + ".[Host] = " + Websites.TABLE_WEBSITES + ".[Host] ";
        }


        private static List<Page> GetPages(DataTable dataTable)
        {
            List<Page> pages = [];
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
            List<Page> pages = [];
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

        public static DataTable GetListingsResponseBodyText(int? top = null)
        {
            string queryTop = string.Empty;
            if (top != null)
            {
                queryTop = " TOP " + top;
            }
            string query =
                "SELECT " + queryTop + " [ResponseBodyText] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [PageType] ='Listing'";

            return new DataBase().QueryTable(query);
        }

        public static DataTable GetResponseBodyText(PageType pageType, int? top = null)
        {
            string queryTop = string.Empty;
            if (top != null)
            {
                queryTop = " TOP " + top;
            }
            string query =
                "SELECT " + queryTop + " [ResponseBodyText] " +
                "FROM " + TABLE_PAGES + " " +
                "WHERE [PageType] = @PageType";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                {"PageType", pageType.ToString() }
            });
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

        public static void UpdateResponseBodyTextHash()
        {
            var pages = GetPages();
            var sync = new object();
            int counter = 0;
            int changed = 0;
            Parallel.ForEach(pages, page =>
            {
                Console.WriteLine(counter + "/" + pages.Count + " Changed: " + changed);
                page.SetResponseBodyTextHash();
                if (page.ResponseBodyTextHasChanged)
                {
                    page.Update();
                    Interlocked.Increment(ref changed);
                }
                Interlocked.Increment(ref counter);
            });
        }

        public static void DeleteNumPagesExceded()
        {
            string query =
                "SELECT [Host] " +
                "FROM " + TABLE_PAGES + " " +
                "GROUP BY [Host] " +
                "HAVING COUNT(*) > " + Configuration.Config.MAX_PAGES_PER_WEBSITE;

            DataTable dataTable = new DataBase().QueryTable(query);
            int total = dataTable.Rows.Count;
            int counter = 0;
            Parallel.ForEach(dataTable.AsEnumerable(), dataRow =>
            {
                Console.WriteLine(counter++ + "/" + total);
                string host = (string)dataRow[0];
                Website website = new(host);
                var pages = website.GetPages();
                int pageCounter = 0;
                foreach (var page in pages)
                {
                    pageCounter++;
                    if (page.IsMainPage())
                    {
                        continue;
                    }
                    if (pageCounter > Configuration.Config.MAX_PAGES_PER_WEBSITE)
                    {
                        page.Delete();
                    }
                }
            });
        }
    }
}
