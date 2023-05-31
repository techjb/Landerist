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
        private static readonly object sync = new();

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

        public void DeleteAndInsert(Uri uri)
        {
            Website website = new(uri);
            DeleteAndInsert(website);
        }

        public static void DeleteAndInsert(Website website)
        {
            website.Delete();
            Insert(website.MainUri);
        }

        public void Insert(string url)
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

                //if (!uri.ToString().Contains("iregua.com"))
                //{
                //    return;
                //}

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
                    IncreaseSkipped();
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
                IncreaseErrorsException();
                Logs.Log.WriteLogErrors(exception);
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

            if (Websites.Websites.ExistsWebsite(uri.Host))
            {
                return false;
            }
            return true;
        }

        private static void InsertWebsite(Website website)
        {
            if (!website.SetMainUriAndStatusCode())
            {
                IncreaseErrorsMainUri();
                return;
            }
            if (!CanInsert(website.MainUri))
            {
                IncreaseSkipped();
                return;
            }            
            if (!website.SetRobotsTxt())
            {
                IncreaseErrorsRobotsTxt();
                return;
            }
            if (!website.SetIpAddress())
            {
                IncreaseErrorsIpAddress();
                return;
            }
            if (!website.Insert())
            {
                IncreaseErrorsInsert();
                return;
            }

            IncreaseInserted();
            InsertedUris.Add(website.MainUri);

            try
            {
                website.InsertMainPage();
                website.InsertPagesFromSiteMap();
            }
            catch (Exception exception)
            {
                IncreaseErrorsException();
                Logs.Log.WriteLogErrors(exception);
            }
        }


        private static void IncreaseErrorsMainUri()
        {
            lock (sync)
            {
                ErrorsMainUri++;
            }
        }

        private static void IncreaseErrorsRobotsTxt()
        {
            lock (sync)
            {
                ErrorsRobotsTxt++;
            }
        }

        private static void IncreaseErrorsIpAddress()
        {
            lock (sync)
            {
                ErrorsIpAddress++;
            }
        }

        private static void IncreaseErrorsException()
        {
            lock (sync)
            {
                ErrorsException++;
            }
        }

        private static void IncreaseErrorsInsert()
        {
            lock (sync)
            {
                ErrorsInsert++;
            }
        }

        private static void IncreaseSkipped()
        {
            lock (sync)
            {
                Skipped++;
            }
        }

        private static void IncreaseInserted()
        {
            lock (sync)
            {
                Inserted++;
            }
        }
    }
}
