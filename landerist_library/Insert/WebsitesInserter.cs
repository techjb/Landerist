using landerist_library.Logs;
using landerist_library.Websites;


namespace landerist_library.Insert
{
    public class WebsitesInserter
    {
        private readonly static HashSet<Uri> InsertedUris = [];

        private static int Inserted = 0;
        private static int ErrorsMainUri = 0;
        private static int ErrorsRobotsTxt = 0;
        private static int ErrorsIpAddress = 0;
        private static int ErrorsInsert = 0;
        private static int ErrorsException = 0;
        private static int Skipped = 0;        
        private static readonly object syncHashSet = new();

        public WebsitesInserter(bool initInsertedUris)
        {
            if (initInsertedUris)
            {
                InitInsertedUris();
            }
        }

        private static void InitInsertedUris()
        {
            var urls = Websites.Websites.GetUrls();
            foreach (var url in urls)
            {
                Uri uri = new(url);
                InsertedUris.Add(uri);
            }
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
            Insert(hashSet);
        }

        private static void Insert(HashSet<Uri> uris)
        {
            int total = uris.Count;
            Parallel.ForEach(uris,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                uri =>
            {
                InsertWebsite(uri);
                Console.WriteLine(
                    "Total: " + total + " " +
                    "Skipped: " + Skipped + " " +
                    "Inserted: " + Inserted + " " +
                    "ErrorsMainUri: " + ErrorsMainUri + " " +
                    "ErrorsRobotsTxt: " + ErrorsRobotsTxt + " " +
                    "ErrorsIpAddress: " + ErrorsIpAddress + " " +
                    "ErrorsInsert: " + ErrorsInsert + " " +
                    "ErrorsException: " + ErrorsException + " ");
            });
        }

        private static bool InsertWebsite(Uri uri)
        {
            try
            {
                if (!CanInsert(uri))
                {
                    Interlocked.Increment(ref Skipped);
                    return false;
                }

                Website website = new()
                {
                    MainUri = uri,
                    Host = uri.Host,
                };
                return InsertWebsite(website);                
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteLogErrors(uri, exception);
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
            lock (syncHashSet)
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
