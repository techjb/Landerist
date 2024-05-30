﻿using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
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

        private readonly static HashSet<string> InsertedHosts = [];

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

        public static void Insert(HashSet<Uri> uris, bool listingExamples = false)
        {
            int total = uris.Count;
            int counter = 0;
            Parallel.ForEach(uris,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                uri =>
            {
                var mainUri = GetMainUri(uri);
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

        public static Uri? GetMainUri(Uri listingExampleUri)
        {
            string mainUriString = listingExampleUri.GetLeftPart(UriPartial.Authority);
            if (Uri.TryCreate(mainUriString, UriKind.Absolute, out Uri? uri))
            {
                return uri;
            }
            return null;
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
