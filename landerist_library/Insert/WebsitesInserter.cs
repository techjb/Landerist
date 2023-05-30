﻿using landerist_library.Websites;
using System;

namespace landerist_library.Insert
{
    public class WebsitesInserter
    {
        private readonly static HashSet<Uri> InsertedUris = new();

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
            int inserted = 0;
            int errors = 0;
            int total = uris.Count;
            object sync = new();

            Parallel.ForEach(uris, 
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 }, 
                uri =>
            {
                bool success = InsertWebsite(uri);
                lock (sync)
                {
                    if (success)
                    {
                        inserted++;
                    }
                    else
                    {
                        errors++;
                    }
                }
                Console.WriteLine("Total: " + total + " Inserted: " + inserted + " Errors: " + errors);
            });
        }

        private static bool InsertWebsite(Uri uri)
        {
            try
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

                Website website = new()
                {
                    MainUri = uri,
                    Host = uri.Host,
                };
                return InsertWebsite(website);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }
            return false;
        }

        private static bool InsertWebsite(Website website)
        {
            if (!website.SetMainUriAndStatusCode())
            {
                return false;
            }            
            if (InsertedUris.Contains(website.MainUri))
            {
                return false;
            }
            if (Websites.Websites.ExistsWebsite(website.Host))
            {
                return false;
            }
            if (!website.SetRobotsTxt())
            {
                return false;
            }
            if (!website.SetIpAddress())
            {
                return false;
            }
            if (!website.Insert())
            {
                return false;
            }

            website.InsertMainPage();
            try
            {
                website.InsertPagesFromSiteMap();
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors(exception);
            }


            InsertedUris.Add(website.MainUri);

            return true;
        }
    }
}
