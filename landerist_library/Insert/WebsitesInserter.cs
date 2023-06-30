using landerist_library.Logs;
using landerist_library.Websites;


namespace landerist_library.Insert
{
    public class WebsitesInserter
    {
        private readonly static HashSet<Uri> InsertedUris = new();

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

        public static void Insert(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                Insert(uri);
            }
        }

        public static void Insert(Uri uri)
        {
            var list = new List<Uri>()
            {
                uri
            };
            Insert(list);
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

        private static void InsertWebsite(Uri uri)
        {
            try
            {
                if (!CanInsert(uri))
                {
                    Interlocked.Increment(ref Skipped);
                    return;
                }

                Website website = new()
                {
                    MainUri = uri,
                    Host = uri.Host,
                };
                InsertWebsite(website);
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteLogErrors(uri, exception);
            }
        }

        private static bool CanInsert(Uri uri)
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

        private static void InsertWebsite(Website website)
        {
            if (!website.SetMainUriAndStatusCode())
            {
                Interlocked.Increment(ref ErrorsMainUri);
                return;
            }
            if (!CanInsert(website.MainUri))
            {
                Interlocked.Increment(ref Skipped);
                return;
            }
            if (!website.SetRobotsTxt())
            {
                Interlocked.Increment(ref ErrorsRobotsTxt);
                return;
            }
            if (!website.SetIpAddress())
            {
                Interlocked.Increment(ref ErrorsIpAddress);
                return;
            }
            if (!website.Insert())
            {
                Interlocked.Increment(ref ErrorsInsert);
                return;
            }

            Interlocked.Increment(ref Inserted);
            lock (syncHashSet)
            {
                InsertedUris.Add(website.MainUri);
            }

            try
            {
                website.InsertMainPage();
                website.InsertPagesFromSiteMap();
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteLogErrors(website.Host, exception);
            }
        }
    }
}
