using landerist_library.Database;
using System.Data;

namespace landerist_library.Websites
{
    public class Websites
    {
        public const string TABLE_WEBSITES = "[WEBSITES]";

        public static HashSet<Website> GetAll()
        {
            var dataTable = GetDataTableAll();
            return GetWebsites(dataTable);
        }

        public static Dictionary<string, Website> GetDicionaryStatusCodeOk()
        {
            Dictionary<string, Website> dictionary = [];
            var websites = GetStatusCodeOk();
            foreach (var website in websites)
            {
                dictionary.Add(website.Host, website);
            }
            return dictionary;
        }

        public static HashSet<Website> GetStatusCodeOk()
        {
            var dataTable = ToDataTableHttpStatusCodeOk();
            return GetWebsites(dataTable);
        }

        public static HashSet<Website> GetStatusCodeNotOk()
        {
            var dataTable = ToDataTableHttpStatusCodeNotOk();
            return GetWebsites(dataTable);
        }

        public static HashSet<Website> GetStatusCodeNull()
        {
            var dataTable = ToDataTableHttpStatusCodeNull();
            return GetWebsites(dataTable);
        }

        private static DataTable GetDataTableAll()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES;
            return new DataBase().QueryTable(query);
        }

        public static DataTable GetListingExampleHtmlNull()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [ListingExampleHtml] is null";
            return new DataBase().QueryTable(query);
        }

        public static DataTable GetDataTableHostMainUri()
        {
            string query =
                "SELECT [Host], [MainUri] " +
                "FROM " + TABLE_WEBSITES;
            return new DataBase().QueryTable(query);
        }


        private static DataTable ToDataTableHttpStatusCodeOk()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] = 200";
            return new DataBase().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNotOk()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] <> 200 AND [HttpStatusCode] IS NOT NULL";
            return new DataBase().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNull()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] IS NULL";
            return new DataBase().QueryTable(query);
        }

        public static Website GetWebsite(Page page)
        {
            return GetWebsite(page.Host);
        }

        public static Website GetWebsite(string host)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE Host = @Host";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });

            var dataRow = dataTable.Rows[0];
            return new Website(dataRow);
        }

        public static bool Exists(string host)
        {
            string query =
                "IF EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + TABLE_WEBSITES + " " +
                "   WHERE Host = @Host) " +
                "SELECT 'true' " +
                "ELSE " +
                "SELECT 'false' ";

            return new DataBase().QueryBool(query, new Dictionary<string, object?> {
                {"Host", host }
            });
        }

        private static HashSet<Website> GetWebsites(DataTable dataTable)
        {
            var hashSet = new HashSet<Website>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new(dataRow);
                hashSet.Add(website);
            }
            return hashSet;
        }

        public static HashSet<string> GetUrls()
        {
            string query =
                "SELECT Uri " +
                "FROM " + TABLE_WEBSITES;
            return new DataBase().QueryHashSet(query);
        }

        public static void SetHttpStatusCodesToAll()
        {
            var websites = GetAll();
            SetHttpStatusCodes(websites);
        }

        public static void SetHttpStatusCodesToNull()
        {
            var websites = GetStatusCodeNull();
            SetHttpStatusCodes(websites);
        }

        private static void SetHttpStatusCodes(HashSet<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int successed = 0;
            int errors = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
                {
                    bool success = website.SetMainUri();
                    if (success)
                    {
                        website.Update();
                    }
                    lock (sync)
                    {
                        counter++;
                        if (success)
                        {
                            successed++;
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                    Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                        "Success: " + successed + " Errors: " + errors + " " + website.MainUri.ToString());
                });
        }

        public static void SetRobotsTxt()
        {
            var websites = GetAll();
            SetRobotsTxt(websites);
        }

        public static void SetRobotsTxtToHttpStatusCodeOk()
        {
            var websites = GetStatusCodeOk();
            SetRobotsTxt(websites);
        }

        private static void SetRobotsTxt(HashSet<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int successed = 0;
            int errors = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1},
                website =>
            {
                bool success = website.SetRobotsTxt();
                if (success)
                {
                    website.Update();
                }
                lock (sync)
                {
                    counter++;
                    if (success)
                    {
                        successed++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                    "Success: " + successed + " Errors: " + errors + " " + website.MainUri.ToString());
            });
        }

        public static void SetIpAdress()
        {
            var websites = GetAll();
            SetIpAdress(websites);
        }

        private static void SetIpAdress(HashSet<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int errors = 0;
            int successed = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
                {
                    bool success = website.SetIpAddress();
                    if (success)
                    {
                        website.Update();
                    }
                    lock (sync)
                    {
                        counter++;
                        if (success)
                        {
                            successed++;
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                    Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                        "Success: " + successed + " Errors: " + errors + " " + website.MainUri.ToString());
                });
        }

        public static void CountCanAccesToMainUri()
        {
            var websites = GetStatusCodeOk();
            int counterYes = 0;
            int counterNo = 0;
            foreach (var website in websites)
            {
                bool canAccess = website.IsMainUriAllowedByRobotsTxt();
                if (canAccess)
                {
                    counterYes++;
                }
                else
                {
                    counterNo++;
                }
                Console.WriteLine("Yes: " + counterYes + " No: " + counterNo + " " + website.MainUri);
            }
        }

        public static void CountRobotsSiteMaps()
        {
            var websites = GetStatusCodeOk();
            int counter = 0;
            foreach (var website in websites)
            {
                counter += website.CountRobotsSiteMaps();
                Console.WriteLine("SiteMaps: " + counter);
            }
        }

        public static void InsertMainPages()
        {
            var websites = GetStatusCodeOk();
            int inserted = 0;
            int errors = 0;
            foreach (var website in websites)
            {
                if (website.InsertMainPage())
                {
                    inserted++;
                }
                else
                {
                    errors++;
                }
                Console.WriteLine("Inserted: " + inserted + " Errors: " + errors + " From: " + websites.Count);
            }
        }

        public static void Delete(Uri uri)
        {
            var website = new Website(uri);
            Delete(website);
        }

        public static void Delete(Website website)
        {
            website.Delete();
        }

        private static bool Delete()
        {
            string query =
             "DELETE FROM " + TABLE_WEBSITES;

            return new DataBase().Query(query);
        }

        public static void DeleteAll()
        {
            Delete();
            Pages.DeleteAll();
            ES_Listings.Delete();
            ES_Media.Delete();
        }

        public static void UpdateNumPages()
        {
            var websites = GetAll();
            int total = websites.Count;
            int counter = 0;
            Parallel.ForEach(websites, website =>
            {
                counter++;
                Console.WriteLine(counter + "/" + total);
                UpdateNumPages(website);
            });
        }

        public static bool UpdateNumPages(Website website)
        {
            int numPages = CalculateNumPages(website);
            string query =
                "UPDATE " + TABLE_WEBSITES + " " +
                "SET [NumPages] = @NumPages " +
                "WHERE [Host] = @Host";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host},
                {"NumPages",numPages }
            });
        }

        private static int CalculateNumPages(Website website)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + Pages.TABLE_PAGES + " " +
                "WHERE [Host] = @Host";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>()
            {
                {"Host", website.Host}
            });
        }

        public static void UpdateRobotsTxt()
        {
            var websites = GetNeedToUpdateRobotsTxt();
            if (websites.Count.Equals(0))
            {
                return;
            }
            Parallel.ForEach(websites, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            }, website =>
            {
                website.SetRobotsTxt();
                website.Update();
            });
        }

        private static HashSet<Website> GetNeedToUpdateRobotsTxt()
        {
            DateTime robotsTxtUpdated = DateTime.Now.AddDays(-Configuration.Config.DAYS_TO_UPDATE_ROBOTS_TXT);

            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [RobotsTxtUpdated] < @RobotsTxtUpdated ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"RobotsTxtUpdated", robotsTxtUpdated },
            });

            return GetWebsites(dataTable);
        }

        public static void UpdateSitemaps()
        {
            var websites = GetNeedToUpdateSitemaps();
            if (websites.Count.Equals(0))
            {
                return;
            }
            Parallel.ForEach(websites, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            }, website =>
            {
                website.SetSitemap();
                website.Update();
            });
        }

        private static HashSet<Website> GetNeedToUpdateSitemaps()
        {
            DateTime sitemapUpdated = DateTime.Now.AddDays(-Configuration.Config.DAYS_TO_UPDATE_SITEMAP);

            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [SitemapUpdated] < @SitemapUpdated ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"SitemapUpdated", sitemapUpdated },
            });

            return GetWebsites(dataTable);
        }

        public static void UpdateIpAddress()
        {
            var websites = GetNeedToUpdateIpAddress();
            if (websites.Count.Equals(0))
            {
                return;
            }
            Parallel.ForEach(websites, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            }, website =>
            {
                website.SetIpAddress();
                website.Update();
            });
        }

        private static HashSet<Website> GetNeedToUpdateIpAddress()
        {
            DateTime ipAddressUpdated = DateTime.Now.AddDays(-Configuration.Config.DAYS_TO_UPDATE_IP_ADDRESS);

            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [IpAddressUpdated] < @IpAddressUpdated ";

            var dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"IpAddressUpdated", ipAddressUpdated },
            });

            return GetWebsites(dataTable);
        }

        public static void DeleteFromFile()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + "HostMainUri.csv";
            DataTable dataTable = Tools.Csv.ToDataTable(file);

            HashSet<string> hosts = [];
            foreach (DataRow row in dataTable.Rows)
            {
                string host = (string)row[0];
                string listingUrl = ((string)row[2]).Trim();
                if (listingUrl.Equals(string.Empty))
                {
                    hosts.Add(host);
                }
            }
            int total = hosts.Count;
            int processed = 0;

            Parallel.ForEach(hosts.AsEnumerable(), host =>
            {
                Website website = new(host);
                if (website.GetNumPages() > 0)
                {
                    website.Delete();
                }
                Interlocked.Increment(ref processed);
                Console.WriteLine(processed + "/" + total);
            });
        }

        public static void UpdateListingExampleUriFromFile()
        {
            string file = Configuration.Config.INSERT_DIRECTORY + "HostMainUri.csv";
            DataTable dataTable = Tools.Csv.ToDataTable(file);
            int total = dataTable.Rows.Count;
            int processed = 0;
            int updated = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                processed++;
                Console.WriteLine(processed + "/" + total + " Updated: " + updated);
                string host = (string)row[0];
                string listingUrl = ((string)row[2]).Trim();
                if (listingUrl.Equals(string.Empty))
                {
                    continue;
                }
                if (Uri.TryCreate(listingUrl, UriKind.Absolute, out Uri? listingExampleUri))
                {
                    Website website = new(host)
                    {
                        ListingExampleUri = listingExampleUri
                    };
                    if (website.Update())
                    {
                        updated++;
                    }
                }
            }
        }

        public static void UpdateListingsExampleHtmlNulls()
        {
            var dataTable = GetListingExampleHtmlNull();
            var websites = GetWebsites(dataTable);
            int total = websites.Count;
            int processed = 0;
            int updated = 0;
            int errors = 0;
            Parallel.ForEach(websites, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
                website =>
            {
                if (website.SetListingExampleHtml())
                {
                    website.Update();
                    Interlocked.Increment(ref updated);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }

                Interlocked.Increment(ref processed);
                Console.WriteLine(processed + "/" + total + " Updated: " + updated + " Errors: " + errors);
            });
        }

        public static void DeleteNullListingExampleHtml()
        {
            var dataTable = GetListingExampleHtmlNull();
            var websites = GetWebsites(dataTable);

            int total = websites.Count;
            int processed = 0;
            int deleted = 0;

            Parallel.ForEach(websites, new ParallelOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount - 1
            },
                website =>
                {
                    if (website.Delete())
                    {
                        Interlocked.Increment(ref deleted);
                    }

                    Interlocked.Increment(ref processed);
                    Console.WriteLine(processed + "/" + total + " Deleted: " + deleted);
                });
        }
    }
}
