using landerist_library.Inserter;
using System.Data;

namespace landerist_library.Websites
{
    public class Websites
    {
        protected const string TABLE_WEBSITES = "[WEBSITES]";

        public static List<Website> GetAll()
        {
            var dataTable = GetDataTableAll();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetHttpStatusCodeOk()
        {
            var dataTable = ToDataTableHttpStatusCodeOk();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetHttpStatusCodeNotOk()
        {
            var dataTable = ToDataTableHttpStatusCodeNotOk();
            return ParseWebsites(dataTable);
        }

        public static List<Website> GetHttpStatusCodeNull()
        {
            var dataTable = ToDataTableHttpStatusCodeNull();
            return ParseWebsites(dataTable);
        }

        private static DataTable GetDataTableAll()
        {
            string query = "SELECT * FROM " + TABLE_WEBSITES;
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeOk()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] = 200";
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNotOk()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] <> 200 AND [HttpStatusCode] IS NOT NULL";
            return new Database().QueryTable(query);
        }

        private static DataTable ToDataTableHttpStatusCodeNull()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_WEBSITES + " " +
                "WHERE [HttpStatusCode] IS NULL";
            return new Database().QueryTable(query);
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
                "FROM " + TABLE_WEBSITES;
            return new Database().QueryHashSet(query);
        }

        public void SetHttpStatusCodesToAll()
        {
            var websites = GetAll();
            SetHttpStatusCodes(websites);
        }

        public void SetHttpStatusCodesToNull()
        {
            var websites = GetHttpStatusCodeNull();
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
                        "Errors: " + erros + " " + website.Uri.ToString());
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
            var websites = GetHttpStatusCodeNotOk();
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
                        "Uris: " + uris.Count + " " + "Errors: " + errors + " " + website.Uri.ToString());
                    try
                    {
                        var uri = website.GetLocation();
                        if (uri == null)
                        {
                            return;
                        }

                        if (uri.Host.Equals(website.Domain))
                        {
                            website.UpdateUri(uri);
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
            var websites = GetAll();
            SetRobotsTxt(websites);
        }

        public void SetRobotsTxtToHttpStatusCodeOk()
        {
            var websites = GetHttpStatusCodeOk();
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
                    "Success: " + successed + " Errors: " + errors + " " + website.Uri.ToString());
            });
        }

        public void SetIpAdressToAll()
        {
            var websites = GetAll();
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
                        "Success: " + successed + " Errors: " + errors + " " + website.Uri.ToString());
                });
        }
    }
}
