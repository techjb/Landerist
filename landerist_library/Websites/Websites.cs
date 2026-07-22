using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Websites
{
    public partial class Websites
    {
        public const string WEBSITES = "[WEBSITES]";
        private static readonly WebsiteQueryRepository WebsiteQueries = new();

        public static HashSet<Website> GetAll()
        {
            var dataTable = GetDataTableAll();
            return GetWebsites(dataTable);
        }

        public static HashSet<string> GetHosts()
        {
            return WebsiteQueries.GetHosts();
        }

        public static Dictionary<string, Website> GetDicionaryStatusCodeOk()
        {
            Dictionary<string, Website> dictionary = new(StringComparer.OrdinalIgnoreCase);
            var websites = GetStatusCodeOk();
            foreach (var website in websites)
            {
                dictionary[website.Host] = website;
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

        public static DataTable GetDataTableAll()
        {
            return WebsiteQueries.GetAll();
        }

        public static DataTable GetDataTableHostMainUri()
        {
            return WebsiteQueries.GetHostMainUri();
        }

        private static DataTable ToDataTableHttpStatusCodeOk()
        {
            return WebsiteQueries.GetHttpStatusCodeOk();
        }

        private static DataTable ToDataTableHttpStatusCodeNotOk()
        {
            return WebsiteQueries.GetHttpStatusCodeNotOk();
        }

        private static DataTable ToDataTableHttpStatusCodeNull()
        {
            return WebsiteQueries.GetHttpStatusCodeNull();
        }

        public static Website GetWebsite(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            return GetWebsite(page.Host);
        }

        public static Website GetWebsite(string host)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(host);

            DataTable dataTable = WebsiteQueries.GetWebsite(host);

            if (dataTable.Rows.Count == 0)
            {
                throw new KeyNotFoundException("Website not found for host: " + host);
            }

            var dataRow = dataTable.Rows[0];
            return WebsiteDataMapper.Map(dataRow);
        }

        public static bool Exists(string host)
        {
            return WebsiteQueries.Exists(host);
        }

        private static HashSet<Website> GetWebsites(DataTable dataTable)
        {
            var hashSet = new HashSet<Website>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = WebsiteDataMapper.Map(dataRow);
                hashSet.Add(website);
            }
            return hashSet;
        }

        public static HashSet<string> GetUrls()
        {
            return WebsiteQueries.GetUrls();
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

            Parallel.ForEach(websites,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM,
                },
                website =>
                {
                    try
                    {
                        bool success = website.SetMainUri();
                        if (success)
                        {
                            global::landerist_library.Websites.Websites.Update(website);
                        }

                        int current = Interlocked.Increment(ref counter);
                        if (success)
                        {
                            Interlocked.Increment(ref successed);
                        }
                        else
                        {
                            Interlocked.Increment(ref errors);
                        }

                        double progressPercentage = Math.Round((double)current * 100 / total, 2);
                        Console.WriteLine(current + "/" + total + " (" + progressPercentage + "%) " +
                            "Success: " + successed + " Errors: " + errors + " " + GetWebsiteDisplayText(website));
                    }
                    finally
                    {
                        website.Dispose();
                    }
                });
        }

        public static void SetRobotsTxt()
        {
            var websites = GetAll();
            SetRobotsTxt(websites);
        }      

        private static void SetRobotsTxt(HashSet<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int successed = 0;
            int errors = 0;

            Parallel.ForEach(websites,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                website =>
                {
                    try
                    {
                        bool success = website.SetRobotsTxt();
                        if (success)
                        {
                            global::landerist_library.Websites.Websites.Update(website);
                        }

                        int current = Interlocked.Increment(ref counter);
                        if (success)
                        {
                            Interlocked.Increment(ref successed);
                        }
                        else
                        {
                            Interlocked.Increment(ref errors);
                        }

                        double progressPercentage = Math.Round((double)current * 100 / total, 2);
                        Console.WriteLine(current + "/" + total + " (" + progressPercentage + "%) " +
                            "Success: " + successed + " Errors: " + errors + " " + GetWebsiteDisplayText(website));
                    }
                    finally
                    {
                        website.Dispose();
                    }
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

            Parallel.ForEach(websites,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
                },
                website =>
                {
                    try
                    {
                        bool success = website.SetIpAddress();
                        if (success)
                        {
                            global::landerist_library.Websites.Websites.Update(website);
                        }

                        int current = Interlocked.Increment(ref counter);
                        if (success)
                        {
                            Interlocked.Increment(ref successed);
                        }
                        else
                        {
                            Interlocked.Increment(ref errors);
                        }

                        double progressPercentage = Math.Round((double)current * 100 / total, 2);
                        Console.WriteLine(current + "/" + total + " (" + progressPercentage + "%) " +
                            "Success: " + successed + " Errors: " + errors + " " + GetWebsiteDisplayText(website));
                    }
                    finally
                    {
                        website.Dispose();
                    }
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
                Console.WriteLine("Yes: " + counterYes + " No: " + counterNo + " " + GetWebsiteDisplayText(website));
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
                if (global::landerist_library.Websites.Websites.InsertMainPage(website))
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
            ArgumentNullException.ThrowIfNull(uri);

            Website website = GetWebsite(uri.Host);
            Delete(website);
        }

        public static void Delete(Website website)
        {
            ArgumentNullException.ThrowIfNull(website);
            global::landerist_library.Websites.Websites.DeleteWithRelations(website);
        }

        private static bool Delete()
        {
            return WebsiteQueries.DeleteAll();
        }

        public static void DeleteAll()
        {
            Delete();
            Pages.Pages.DeleteAll();
            DeleteAllListings();
        }

        public static void DeleteAllListings()
        {
            ES_Listings.Delete();
            ES_Media.Delete();
            ES_Sources.Delete();
        }
       
        public static void UpdateRobotsTxt()
        {
            var websites = GetNeedToUpdateRobotsTxt();
            if (websites.Count.Equals(0))
            {
                return;
            }

            int counter = 0;
            Console.WriteLine("Updating robots.txt of " + websites.Count + " websites");
            Parallel.ForEach(websites, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, website =>
            {
                try
                {
                    website.SetRobotsTxt();
                    global::landerist_library.Websites.Websites.Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });

            //Logs.Log.WriteLogInfo("service", "Updated Robots.txt " + counter + "/" + websites.Count);
        }

        private static HashSet<Website> GetNeedToUpdateRobotsTxt()
        {
            DateTime robotsTxtUpdatedSpecialRules = DateTime.Now.AddDays(-1);

            var dataTable = WebsiteQueries.GetNeedToUpdateRobotsTxt(robotsTxtUpdatedSpecialRules);
            return GetWebsites(dataTable);
        }

        public static void UpdateSitemaps()
        {
            var websites = GetNeedToUpdateSitemaps();
            if (websites.Count.Equals(0))
            {
                return;
            }

            int counter = 0;
            Console.WriteLine("Updating sitemaps of " + websites.Count + " websites");
            Parallel.ForEach(websites, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, website =>
            {
                try
                {
                    website.ReadSitemap();
                    Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });

            //Logs.Log.WriteLogInfo("service", "Updated Sitemaps " + counter + "/" + websites.Count);
        }

        private static HashSet<Website> GetNeedToUpdateSitemaps()
        {
            DateTime sitemapUpdatedSpecialRules = DateTime.Now.AddDays(-1);

            var dataTable = WebsiteQueries.GetNeedToUpdateSitemaps(sitemapUpdatedSpecialRules);
            return GetWebsites(dataTable);
        }

        public static void UpdateIpAddress()
        {
            var websites = GetNeedToUpdateIpAddress();
            if (websites.Count.Equals(0))
            {
                return;
            }

            int counter = 0;
            Console.WriteLine("Updating ip address of " + websites.Count + " websites");
            Parallel.ForEach(websites, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
            }, website =>
            {
                try
                {
                    website.SetIpAddress();
                    global::landerist_library.Websites.Websites.Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });

            //Logs.Log.WriteLogInfo("service", "Updated IpAddress " + counter + "/" + websites.Count);
        }

        private static HashSet<Website> GetNeedToUpdateIpAddress()
        {
            DateTime ipAddressUpdated = DateTime.Now.AddDays(-1);

            var dataTable = WebsiteQueries.GetNeedToUpdateIpAddress(ipAddressUpdated);
            return GetWebsites(dataTable);
        }

        public static void DeleteFromFile()
        {
            string file = AppConfig.INSERT_DIRECTORY + "HostMainUri.csv";
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
                Website website = GetWebsite(host);
                try
                {
                    if (global::landerist_library.Websites.Websites.GetNumPages(website) > 0)
                    {
                        global::landerist_library.Websites.Websites.DeleteWithRelations(website);
                    }
                }
                finally
                {
                    website.Dispose();
                }

                int current = Interlocked.Increment(ref processed);
                Console.WriteLine(current + "/" + total);
            });
        }       

        private static string GetWebsiteDisplayText(Website website)
        {
            return website.MainUri?.ToString() ?? website.Host;
        }
    }
}
