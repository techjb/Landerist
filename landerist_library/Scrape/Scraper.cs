﻿using landerist_library.Websites;
using System.Data;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private readonly IpHostBlocker TempBlocker = new();

        private readonly object SyncTempBlocker = new();

        private readonly HashSet<Page> PendingPages = new();

        private readonly object SyncPendingPages = new();

        private int Sucess = 0;

        private int Errors = 0;

        private readonly object SyncCounter = new();

        private readonly object SyncSuccessErrros = new();

        public Scraper()
        {

        }

        public void ScrapeAllPages()
        {
            var dictionary = Websites.Websites.GetDicionaryStatusCodeOk();
            var dataTable = Pages.GetAll();
            var pages = GetPages(dictionary, dataTable);
            ScrapePages(pages);
        }

        private static HashSet<Page> GetPages(Dictionary<string, Website> dictionary, DataTable dataTable)
        {
            HashSet<Page> pages = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var host = (string)dataRow["host"];
                if (!dictionary.ContainsKey(host))
                {
                    continue;
                }
                var website = dictionary[host];
                Page page = new(website, dataRow);
                pages.Add(page);
            }
            return pages;
        }

        public void ScrapeMainPage(Website website)
        {
            var page = new Page(website);
            ScrapePage(page);
        }

        public void ScrapePages(Website website)
        {
            var pages = website.GetPages();
            ScrapePages(pages);
        }

        public void ScrapeNonScrapped(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeNonScrapped(website, recursive);
        }
        public void ScrapeNonScrapped(Website website, bool recursive)
        {
            var pages = website.GetNonScrapedPages();
            if (pages.Count == 0)
            {
                return;
            }
            ScrapePages(pages);
            if (recursive)
            {
                ScrapeNonScrapped(website, recursive);
            }
        }

        // Can be infinite loops
        public void ScrapeUnknowIsListing(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeUnknowIsListing(website, recursive);
        }

        public void ScrapeUnknowIsListing(Website website, bool recursive)
        {
            var pages = website.GetUnknowIsListingPages();
            if (pages.Count == 0)
            {
                return;
            }
            ScrapePages(pages);
            if (recursive)
            {
                ScrapeUnknowIsListing(website, recursive);
            }
        }

        public void ScrapeIsNotListing(Uri uri, bool recursive = false)
        {
            Website website = new(uri);
            ScrapeIsNotListing(website, recursive);
        }

        public void ScrapeIsNotListing(Website website, bool recursive)
        {
            var pages = website.GetIsNotListingPages();
            if (pages.Count == 0)
            {
                return;
            }
            ScrapePages(pages);
            if (recursive)
            {
                ScrapeIsNotListing(website, recursive);
            }
        }

        private void ScrapePages(List<Page> pages)
        {
            HashSet<Page> hashSet = new(pages, new PageComparer());
            ScrapePages(hashSet);
        }

        private void ScrapePages(HashSet<Page> pages)
        {
            PendingPages.Clear();
            int TotalPages = pages.Count;
            int Counter = 0;
            Parallel.ForEach(pages,
                new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    lock (SyncCounter)
                    {
                        Counter++;
                    }
                    Console.WriteLine(
                        "Scrapped: " + Counter + "/" + TotalPages + " Pending: " + PendingPages.Count + " " +
                        "Success: " + Sucess + " Errors: " + Errors);

                    ScrapePage(page);
                });

            if (PendingPages.Count > 0)
            {
                Thread.Sleep(2000);
                List<Page> newList = new(PendingPages);
                ScrapePages(newList);
            }
        }

        public void ScrapePage(Page page)
        {
            if (!page.CanScrape())
            {
                return;
            }
            if (TempBlocker.IsBlocked(page.Website))
            {
                lock (SyncPendingPages)
                {
                    PendingPages.Add(page);
                }
                return;
            }
            AddToBlocker(page);
            bool sucess = new PageScraper(page).Scrape();
            AddSuccessError(sucess);
        }
        private void AddToBlocker(Page page)
        {
            lock (SyncTempBlocker)
            {
                TempBlocker.Add(page.Website);
            }
        }

        private void AddSuccessError(bool success)
        {
            lock (SyncSuccessErrros)
            {
                if (success)
                {
                    Sucess++;
                }
                else
                {
                    Errors++;
                }
            }
        }
    }
}
