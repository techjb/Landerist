using landerist_library.Inserter;
using System.Data;

namespace landerist_library.Websites
{
    public class Websites: WebBase
    {
        public static List<Website> AllWebsites()
        {
            var dataTable = GetDataTableAll();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeOk()
        {
            var dataTable = ToDataTableHttpStatusCodeOk();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeNotOk()
        {
            var dataTable = ToDataTableHttpStatusCodeNotOk();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeNull()
        {
            var dataTable = ToDataTableHttpStatusCodeNull();
            return ParseWebsites(dataTable);
        }

        private static DataTable GetDataTableAll()
        {
            string query = "SELECT * FROM " + WEBSITES;
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeOk()
        {
            string query =
                "SELECT * " +
                "FROM " + WEBSITES + " " +
                "WHERE [HttpStatusCode] = 200";
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNotOk()
        {
            string query =
                "SELECT * " +
                "FROM " + WEBSITES + " " +
                "WHERE [HttpStatusCode] <> 200 AND [HttpStatusCode] IS NOT NULL";
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNull()
        {
            string query =
                "SELECT * " +
                "FROM " + WEBSITES + " " +
                "WHERE [HttpStatusCode] IS NULL";
            return new Database().QueryTable(query);
        }

        public static Website? GetWebsite(string host)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + WEBSITES + " " +
                "WHERE Host = @Host";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object> {
                {"Host", host }
            });

            if (dataTable.Rows.Count.Equals(1))
            {
                return new Website(dataTable.Rows[0]);
            }
            return null;
        }

        private static List<Website> ParseWebsites(DataTable dataTable)
        {
            var list = new List<Website>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new(dataRow);
                list.Add(website);
            }
            return list;
        }

        public static HashSet<string> GetUris()
        {
            string query =
                "SELECT Uri " +
                "FROM " + WEBSITES;
            return new Database().QueryHashSet(query);
        }

        public void SetHttpStatusCodesToAll()
        {
            var websites = AllWebsites();
            SetHttpStatusCodes(websites);
        }

        public void SetHttpStatusCodesToNull()
        {
            var websites = GetStatusCodeNull();
            SetHttpStatusCodes(websites);
        }

        private void SetHttpStatusCodes(List<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int erros = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
                {
                    lock (sync)
                    {
                        counter++;
                    }
                    double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                    Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                        "Errors: " + erros + " " + website.MainUri.ToString());
                    try
                    {
                        website.SetHttpStatusCode();
                    }
                    catch
                    {
                        erros++;
                    }
                });
        }

        public void InsertUpdateUrisFromNotOk()
        {
            var websites = GetStatusCodeNotOk();
            InsertUpdateUris(websites);
        }

        private void InsertUpdateUris(List<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int errors = 0;
            var sync = new object();
            List<Uri> uris = new();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
                {
                    lock (sync)
                    {
                        counter++;
                    }
                    double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                    Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                        "Uris: " + uris.Count + " " + "Errors: " + errors + " " + website.MainUri.ToString());
                    try
                    {
                        var uri = website.GetResponseLocation();
                        if (uri == null)
                        {
                            return;
                        }

                        if (uri.Host.Equals(website.Host))
                        {
                            website.UpdateMainUri(uri);
                            return;
                        }
                        if (!uris.Contains(uri))
                        {
                            lock (sync)
                            {
                                uris.Add(uri);
                            }
                        }
                    }
                    catch
                    {
                        errors++;
                    }
                });
            new UrisInserter().Insert(uris);
        }

        public void SetRobotsTxtToAll()
        {
            var websites = AllWebsites();
            SetRobotsTxt(websites);
        }

        public void SetRobotsTxtToHttpStatusCodeOk()
        {
            var websites = GetStatusCodeOk();
            SetRobotsTxt(websites);
        }

        private void SetRobotsTxt(List<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int successed = 0;
            int errors = 0;
            var sync = new object();
            Parallel.ForEach(websites, website =>
            {
                bool success = false;
                try
                {
                    success = website.SetRobotsTxt();
                }
                catch
                {
                    errors++;
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

        public void SetIpAdressToAll()
        {
            var websites = AllWebsites();
            SetIpAdress(websites);
        }

        private void SetIpAdress(List<Website> websites)
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
                    bool success = false;
                    try
                    {
                        success = website.SetIpAddress();
                    }
                    catch
                    {
                        errors++;
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

        public void CountCanAccesToMainUri()
        {
            var websites = GetStatusCodeOk();
            int counterYes = 0;
            int counterNo = 0;
            foreach(var website in websites)
            {
                bool canAccess = website.CanAccessMainUri();
                if(canAccess)
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

        public void CountSiteMaps()
        {
            var websites = GetStatusCodeOk();
            int counter = 0;
            foreach (var website in websites)
            {
                counter += website.CountRobotsSiteMaps();
                Console.WriteLine("SiteMaps: " + counter);
            }
        }

        public void CalculateHashes()
        {
            var websites = GetStatusCodeOk();            
            foreach (var website in websites)
            {
                var page = new Page(website.MainUri);               
                Console.WriteLine(page.UriHash + " " + page.Uri);
            }
        }

        public void InsertMainPages()
        {
            var websites = GetStatusCodeOk();
            int inserted = 0;
            int errors = 0;
            foreach (var website in websites)
            {
                var page = new Page(website);
                if (page.Insert())
                {
                    inserted++;
                }
                else
                {
                    errors++;
                }
                Console.WriteLine("Inserted: " + inserted + " Errors: "+ errors +" From: " + websites.Count);
            }
        }

        public void RemoveBlockedDomains()
        {
            var websites = AllWebsites();
            int counter = 0;
            foreach(var website in websites)
            {
                if (BlockedDomains.IsBlocked(website.MainUri))
                {
                    website.Remove();
                    counter++;
                }
            }
            Console.WriteLine("Blocked: " + counter);
        }
    }
}
