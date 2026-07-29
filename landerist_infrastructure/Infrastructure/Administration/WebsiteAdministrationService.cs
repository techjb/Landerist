using landerist_library.Websites;
using landerist_library.Configuration;
using landerist_library.Pages;
using System.Data;

namespace landerist_library.Infrastructure.Administration
{
    public sealed partial class WebsiteAdministrationService
    {
        public const string WEBSITES = "[WEBSITES]";
        public HashSet<Website> GetAll() => [.. Catalog.GetAll()];

        public HashSet<string> GetHosts() => [.. Catalog.GetHosts()];

        public Dictionary<string, Website> GetDicionaryStatusCodeOk()
        {
            Dictionary<string, Website> dictionary = new(StringComparer.OrdinalIgnoreCase);
            foreach (Website website in GetStatusCodeOk())
            {
                dictionary[website.Host] = website;
            }
            return dictionary;
        }

        public HashSet<Website> GetStatusCodeOk() =>
            [.. Catalog.GetWithSuccessfulStatus()];

        public HashSet<Website> GetStatusCodeNotOk() =>
            [.. Catalog.GetWithUnsuccessfulStatus()];

        public HashSet<Website> GetStatusCodeNull() =>
            [.. Catalog.GetWithoutStatus()];

        public Website GetWebsite(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            return Catalog.Get(page.Host);
        }

        public Website GetWebsite(string host) => Catalog.Get(host);

        public bool Exists(string host) => Catalog.Exists(host);

        public IReadOnlyCollection<string> GetUrls() => [.. Catalog.GetUrls()];
        public void SetHttpStatusCodesToAll()
        {
            var websites = GetAll();
            SetHttpStatusCodes(websites);
        }

        public void SetHttpStatusCodesToNull()
        {
            var websites = GetStatusCodeNull();
            SetHttpStatusCodes(websites);
        }

        private void SetHttpStatusCodes(HashSet<Website> websites)
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
                        bool success = Network.RefreshMainUri(website);
                        if (success)
                        {
                            Update(website);
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

        public void SetRobotsTxt()
        {
            var websites = GetAll();
            SetRobotsTxt(websites);
        }      

        private void SetRobotsTxt(HashSet<Website> websites)
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
                        bool success = Network.RefreshRobotsTxt(website);
                        if (success)
                        {
                            Update(website);
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

        public void SetIpAdress()
        {
            var websites = GetAll();
            SetIpAdress(websites);
        }

        private void SetIpAdress(HashSet<Website> websites)
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
                        bool success = Network.RefreshIpAddress(website);
                        if (success)
                        {
                            Update(website);
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

        public void CountCanAccesToMainUri()
        {
            var websites = GetStatusCodeOk();
            int counterYes = 0;
            int counterNo = 0;
            foreach (var website in websites)
            {
                bool canAccess = RobotsPolicy.IsAllowed(website, website.MainUri);
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

        public void CountRobotsSiteMaps()
        {
            var websites = GetStatusCodeOk();
            int counter = 0;
            foreach (var website in websites)
            {
                counter += RobotsPolicy.GetSitemapUrls(website).Count;
                Console.WriteLine("SiteMaps: " + counter);
            }
        }

        public void InsertMainPages()
        {
            var websites = GetStatusCodeOk();
            int inserted = 0;
            int errors = 0;
            foreach (var website in websites)
            {
                if (InsertMainPage(website))
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

        public void Delete(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            Website website = GetWebsite(uri.Host);
            Delete(website);
        }

        public void Delete(Website website)
        {
            ArgumentNullException.ThrowIfNull(website);
            DeleteWithRelations(website);
        }

        private bool Delete()
        {
            return Maintenance.DeleteAll();
        }

        public void DeleteAll()
        {
            Delete();
            PageMaintenance.DeleteAll();
            DeleteAllListings();
        }

        public void DeleteAllListings()
        {
            ListingMaintenance.DeleteAll();
        }
       
        public void UpdateRobotsTxt()
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
                    Network.RefreshRobotsTxt(website);
                    Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });
        }

        private HashSet<Website> GetNeedToUpdateRobotsTxt()
        {
            DateTime robotsTxtUpdatedSpecialRules = DateTime.Now.AddDays(-1);

            return [.. Catalog.GetNeedingRobotsTxtUpdate(robotsTxtUpdatedSpecialRules)];
        }

        public void UpdateSitemaps()
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
                    Sitemaps.RefreshSitemap(website);
                    Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });
        }

        private HashSet<Website> GetNeedToUpdateSitemaps()
        {
            DateTime sitemapUpdatedSpecialRules = DateTime.Now.AddDays(-1);

            return [.. Catalog.GetNeedingSitemapUpdate(sitemapUpdatedSpecialRules)];
        }

        public void UpdateIpAddress()
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
                    Network.RefreshIpAddress(website);
                    Update(website);
                    Interlocked.Increment(ref counter);
                }
                finally
                {
                    website.Dispose();
                }
            });
        }

        private HashSet<Website> GetNeedToUpdateIpAddress()
        {
            DateTime ipAddressUpdated = DateTime.Now.AddDays(-1);

            return [.. Catalog.GetNeedingIpAddressUpdate(ipAddressUpdated)];
        }

        public void DeleteFromFile()
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
                    if (GetNumPages(website) > 0)
                    {
                        DeleteWithRelations(website);
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

        private string GetWebsiteDisplayText(Website website)
        {
            return website.MainUri?.ToString() ?? website.Host;
        }
    }
}

