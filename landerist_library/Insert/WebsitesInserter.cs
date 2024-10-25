using landerist_library.Configuration;
using landerist_library.Logs;
using landerist_library.Websites;
using System.Data;


namespace landerist_library.Insert
{
    public class WebsitesInserter
    {
        private static int Inserted = 0;
        private static int ErrorsMainUri = 0;
        private static int ErrorsRobotsTxt = 0;
        private static int ErrorsIpAddress = 0;
        private static int ErrorsInsert = 0;
        private static int ErrorsException = 0;
        private static int Skipped = 0;
        private static readonly object SyncHashSet = new();

        private readonly static HashSet<Uri> InsertedUris = [];

        public WebsitesInserter(bool initialize)
        {
            if (initialize)
            {
                Init();
            }
        }

        private static void Init()
        {
            var urls = Websites.Websites.GetUrls();
            foreach (var url in urls)
            {
                Uri uri = new(url);
                InsertedUris.Add(uri);
            }
        }

        protected static HashSet<Uri> ToList(DataTable dataTable, string columnName)
        {
            Console.WriteLine("Parsing to list ..");
            HashSet<Uri> uris = [];
            HashSet<string> hosts = [];

            foreach (DataRow row in dataTable.Rows)
            {
                string url = row[columnName].ToString() ?? string.Empty;
                if (url.Equals(string.Empty))
                {
                    continue;
                }
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                try
                {
                    Uri uri = new(url);
                    if (hosts.Add(uri.Host))
                    {
                        uris.Add(uri);
                    }
                }
                catch
                {
                }
            }
            return uris;
        }

        public static void DeleteAndInsert(Uri uri)
        {
            Website website = new(uri);
            DeleteAndInsert(website);
        }

        public static void DeleteAndInsert(Website website)
        {
            website.Delete();
            Insert(website.MainUri);
        }

        public static bool Insert(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return Insert(uri);
            }
            return false;
        }

        public static bool Insert(Uri uri)
        {
            return InsertWebsite(uri);
        }

        public static void Insert(List<Uri> uris)
        {
            HashSet<Uri> hashSet = new(uris);
            Insert(hashSet, false);
        }

        public static void Insert(HashSet<Uri> uris, bool listingExamples)
        {
            int total = uris.Count;
            int counter = 0;
            Parallel.ForEach(uris,
                new ParallelOptions() { MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM },
                uri =>
            {
                var mainUri = GetSuggestedMainUri(uri);
                InsertWebsite(mainUri, listingExamples ? uri : null);
                Interlocked.Increment(ref counter);
                Console.WriteLine(
                    "Processed: " + counter + "/" + total + " " +
                    "Skipped: " + Skipped + " " +
                    "Inserted: " + Inserted + " " +
                    "ErrorsMainUri: " + ErrorsMainUri + " " +
                    "ErrorsRobotsTxt: " + ErrorsRobotsTxt + " " +
                    "ErrorsIpAddress: " + ErrorsIpAddress + " " +
                    "ErrorsInsert: " + ErrorsInsert + " " +
                    "ErrorsException: " + ErrorsException + " ");
            });
        }

        public static Uri? GetSuggestedMainUri(Uri listingExampleUri)
        {
            string mainUriString = listingExampleUri.GetLeftPart(UriPartial.Authority);
            if (Uri.TryCreate(mainUriString, UriKind.Absolute, out Uri? uri))
            {
                return uri;
            }
            return null;
        }

        private static bool InsertWebsite(Uri? mainUri, Uri? listinExampleUri = null)
        {
            if (mainUri == null)
            {
                return false;
            }
            try
            {
                if (!CanInsert(mainUri))
                {
                    Interlocked.Increment(ref Skipped);
                    return false;
                }

                Website website = new()
                {
                    MainUri = mainUri,
                    Host = mainUri.Host,
                };
                if (listinExampleUri != null)
                {
                    website.SetListingExampleUri(listinExampleUri);
                }
                return InsertWebsite(website);
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteLogErrors(mainUri, exception);
            }
            return false;
        }

        public static bool InsertFromListingExampleUri(Uri listingExampleUri)
        {
            string uri = listingExampleUri.GetLeftPart(UriPartial.Authority);
            if (Insert(uri))
            {
                Website website = new(listingExampleUri.Host);
                return website.UpdateListingExample(listingExampleUri);
            }
            return false;
        }

        public static bool CanInsert(Uri uri)
        {
            if (BlockedDomains.IsBlocked(uri))
            {
                return false;
            }

            if (InsertedUris.Contains(uri))
            {
                return false;
            }

            if (Websites.Websites.Exists(uri.Host))
            {
                return false;
            }
            return true;
        }

        private static bool InsertWebsite(Website website)
        {
            if (!website.SetMainUri())
            {
                Interlocked.Increment(ref ErrorsMainUri);
                return false;
            }
            if (!CanInsert(website.MainUri))
            {
                Interlocked.Increment(ref Skipped);
                return false;
            }
            if (!website.SetRobotsTxt())
            {
                Interlocked.Increment(ref ErrorsRobotsTxt);
                return false;
            }
            if (!website.SetIpAddress())
            {
                Interlocked.Increment(ref ErrorsIpAddress);
                return false;
            }
            if (!website.Insert())
            {
                Interlocked.Increment(ref ErrorsInsert);
                return false;
            }

            Interlocked.Increment(ref Inserted);
            lock (SyncHashSet)
            {
                InsertedUris.Add(website.MainUri);
            }

            try
            {
                website.InsertMainPage();
                website.SetSitemap();
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteLogErrors(website.Host, exception);
            }
            return true;
        }
    }
}
