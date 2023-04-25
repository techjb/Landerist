using landerist_library.Database;
using System.Data;

namespace landerist_library.Websites
{
    public class Websites
    {
        public const string TABLE_WEBSITES = "[WEBSITES]";
        public static List<Website> AllWebsites()
        {
            var dataTable = GetDataTableAll();
            return GetWebsites(dataTable);
        }

        public static Dictionary<string, Website> GetDicionaryStatusCodeOk()
        {
            Dictionary<string, Website> dictionary = new();
            var websites = GetStatusCodeOk();
            foreach (var website in websites)
            {
                dictionary.Add(website.Host, website);
            }
            return dictionary;
        }

        public static List<Website> GetStatusCodeOk()
        {
            var dataTable = ToDataTableHttpStatusCodeOk();
            return GetWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeNotOk()
        {
            var dataTable = ToDataTableHttpStatusCodeNotOk();
            return GetWebsites(dataTable);
        }

        public static List<Website> GetStatusCodeNull()
        {
            var dataTable = ToDataTableHttpStatusCodeNull();
            return GetWebsites(dataTable);
        }

        private static DataTable GetDataTableAll()
        {
            string query = "SELECT * FROM " + TABLE_WEBSITES;
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

        public static Website? GetWebsite(string host)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE Host = @Host";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Host", host }
            });

            if (dataTable.Rows.Count.Equals(1))
            {
                var dataRow = dataTable.Rows[0];
                return new Website(dataRow);
            }
            return null;
        }

        private static List<Website> GetWebsites(DataTable dataTable)
        {
            var list = new List<Website>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                Website website = new(dataRow);
                list.Add(website);
            }
            return list;
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
            var websites = AllWebsites();
            SetHttpStatusCodes(websites);
        }

        public static void SetHttpStatusCodesToNull()
        {
            var websites = GetStatusCodeNull();
            SetHttpStatusCodes(websites);
        }

        private static void SetHttpStatusCodes(List<Website> websites)
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
                    bool success = website.SetHttpStatusCode();
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
            var websites = AllWebsites();
            SetRobotsTxt(websites);
        }

        public static void SetRobotsTxtToHttpStatusCodeOk()
        {
            var websites = GetStatusCodeOk();
            SetRobotsTxt(websites);
        }

        private static void SetRobotsTxt(List<Website> websites)
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
                if(success)
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
            var websites = AllWebsites();
            SetIpAdress(websites);
        }

        private static void SetIpAdress(List<Website> websites)
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
                bool canAccess = website.IsMainUriAllowed();
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

        public static void CountSiteMaps()
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
    }
}
