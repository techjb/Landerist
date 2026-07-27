using landerist_library.Websites;
using landerist_library.Insert;
using landerist_library.Application.Administration;
using landerist_library.Application.Websites;
using landerist_library.Logs;
using System.Data;


namespace landerist_library.Infrastructure.Administration
{
    public class WebsitesInserter
    {
        private int Inserted = 0;
        private int ErrorsMainUri = 0;
        private int ErrorsRobotsTxt = 0;
        private int ErrorsIpAddress = 0;
        private int ErrorsInsert = 0;
        private int ErrorsException = 0;
        private int Skipped = 0;
        private readonly object SyncHashSet = new();

        private readonly HashSet<Uri> InsertedUris = [];

        private readonly IWebsiteAdministrationService _websites;
        private readonly IWebsiteNetworkService _network;

        public WebsitesInserter(bool initialize, IWebsiteAdministrationService websites, IWebsiteNetworkService network)
        {
            _websites = websites;
            _network = network;
            if (initialize)
            {
                Init();
            }
        }

        private void Init()
        {
            var urls = _websites.GetUrls();
            foreach (var url in urls)
            {
                Uri uri = new(url);
                InsertedUris.Add(uri);
            }
        }

        protected HashSet<Uri> ToList(DataTable dataTable, string columnName)
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

        public void DeleteAndInsert(Uri uri)
        {
            Website website = new(uri);
            DeleteAndInsert(website);
        }

        public void DeleteAndInsert(Website website)
        {
            _websites.DeleteWithRelations(website);
            Insert(website.MainUri);
        }

        public bool Insert(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            {
                return Insert(uri);
            }
            return false;
        }

        public bool Insert(Uri uri)
        {
            return InsertWebsite(uri);
        }

        public bool InsertWebsite(
            string mainUri,
            string? listingUrlRegex,
            string? sitemapUrlRegex)
        {
            if (!Uri.TryCreate(mainUri, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            return InsertWebsite(
                mainUri,
                uri.Host,
                listingUrlRegex,
                listingUrlRegex,
                sitemapUrlRegex,
                "Unknown,Beacon,Document,StyleSheet,Script,TextTrack,Xhr,Fetch,EventSource,WebSocket,Manifest,Ping,Other",
                null,
                null,
                null);
        }

        public bool InsertWebsite(
            string mainUri,
            string host,
            string? listingUrlRegex,
            string? indexUrlRegex,
            string? sitemapUrlRegex,
            string? allowedResourceTypes = null,
            string? userAgent = null,
            string? httpRequestHeaders = null,
            string? blockedDomains = null)
        {
            if (!Uri.TryCreate(mainUri, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            Website website = new()
            {
                MainUri = uri,
                Host = host.Trim(),
                ListingUrlRegex = NullIfWhiteSpace(listingUrlRegex),
                IndexUrlRegex = NullIfWhiteSpace(indexUrlRegex),
                SitemapUrlRegex = NullIfWhiteSpace(sitemapUrlRegex),
                AllowedResourceTypes = NullIfWhiteSpace(allowedResourceTypes),
                BlockedDomains = NullIfWhiteSpace(blockedDomains),
                UserAgent = NullIfWhiteSpace(userAgent),
                HttpRequestHeaders = NullIfWhiteSpace(httpRequestHeaders),
                HtmlIndexingEnabled = false,
            };


            if (!_network.RefreshRobotsTxt(website))
            {
                Console.WriteLine("Error setting robots.txt for " + website.MainUri);
            }
            if (!_network.RefreshIpAddress(website))
            {
                Console.WriteLine("Error setting IP address for " + website.MainUri);
            }
            if (!_websites.Insert(website))
            {
                Console.WriteLine("Error inserting website " + website.MainUri);
                return false;
            }
            return true;

        }

        public void Insert(List<Uri> uris)
        {
            HashSet<Uri> hashSet = [.. uris];
            Insert(hashSet);
        }

        public void Insert(HashSet<Uri> uris)
        {
            int total = uris.Count;
            int counter = 0;
            Parallel.ForEach(uris,
                new ParallelOptions()
                {
                    //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM 
                },
                uri =>
            {
                var mainUri = GetSuggestedMainUri(uri);
                InsertWebsite(mainUri);
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

        public Uri? GetSuggestedMainUri(Uri sourceUri)
        {
            string mainUriString = sourceUri.GetLeftPart(UriPartial.Authority);
            if (Uri.TryCreate(mainUriString, UriKind.Absolute, out Uri? uri))
            {
                return uri;
            }
            return null;
        }

        private bool InsertWebsite(Uri? mainUri)
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
                return InsertWebsite(website);
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteError(mainUri, exception);
            }
            return false;
        }

        public bool CanInsert(Uri uri)
        {
            if (BlockedDomains.IsBlocked(uri))
            {
                return false;
            }

            if (InsertedUris.Contains(uri))
            {
                return false;
            }

            if (_websites.Exists(uri.Host))
            {
                return false;
            }
            return true;
        }

        private string? NullIfWhiteSpace(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private bool InsertWebsite(Website website)
        {
            if (!_network.RefreshMainUri(website))
            {
                Interlocked.Increment(ref ErrorsMainUri);
                return false;
            }
            if (!CanInsert(website.MainUri))
            {
                Interlocked.Increment(ref Skipped);
                return false;
            }
            if (!_network.RefreshRobotsTxt(website))
            {
                Interlocked.Increment(ref ErrorsRobotsTxt);
                return false;
            }
            if (!_network.RefreshIpAddress(website))
            {
                Interlocked.Increment(ref ErrorsIpAddress);
                return false;
            }
            if (!_websites.Insert(website))
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
                _websites.InsertMainPage(website);
                website.ReadSitemap();
                _websites.Update(website);
            }
            catch (Exception exception)
            {
                Interlocked.Increment(ref ErrorsException);
                Log.WriteError(website.Host, exception);
            }
            return true;
        }
    }
}
