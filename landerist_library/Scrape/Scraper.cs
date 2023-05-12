using landerist_library.Websites;
using System.Data;
using System.Net.Http.Headers;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private readonly IpHostBlocker IpHostBlocker = new();

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

        public void ScrapeMainPage(Website website)
        {
            var page = new Page(website);
            Scrape(page);
        }

        public void ScrapePages(Website website)
        {
            var pages = website.GetPages();
            Scrape(pages);
        }

        public void ScrapeNonScrapped(bool recursive = false)
        {
            List<Page> pages = Pages.GetNonScraped();
            if (pages.Count.Equals(0))
            {
                return;
            }
            Scrape(pages);
            if (recursive)
            {
                ScrapeNonScrapped(recursive);
            }
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
            Scrape(pages);
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
            Scrape(pages);
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
            Scrape(pages);
            if (recursive)
            {
                ScrapeIsNotListing(website, recursive);
            }
        }

        private void Scrape(List<Page> pages)
        {
            HashSet<Page> hashSet = new(pages, new PageComparer());
            Scrape(hashSet);
        }

        private void Scrape(HashSet<Page> pages)
        {
            pages.RemoveWhere(p => !p.CanScrape());
            PendingPages.Clear();
            int totalPages = pages.Count;
            int Counter = 0;
            DateTime dateStart = DateTime.Now;
            Parallel.ForEach(pages,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    lock (SyncCounter)
                    {
                        Counter++;
                    }                    
                    Console.WriteLine(
                        "Scrapped: " + Counter + "/" + totalPages + " Pending: " + PendingPages.Count + " " +
                        "Success: " + Sucess + " Errors: " + Errors);

                    if (IpHostBlocker.IsBlocked(page.Website))
                    {
                        AddToPendingPages(page);
                        return;
                    }
                    Scrape(page);
                });

            ScrapePendingPages(dateStart);
        }

        private void ScrapePendingPages(DateTime dateStart)
        {
            if (PendingPages.Count.Equals(0))
            {
                return;
            }
            if (dateStart.AddSeconds(2) > DateTime.Now)
            {
                Thread.Sleep(2000);
            }
            HashSet<Page> newList = new(PendingPages);
            Scrape(newList);
        }

        public void Scrape(Uri uri)
        {
            var page = new Page(uri);
            Scrape(page);
        }

        public void Scrape(Page page)
        {
            AddToBlocker(page);
            bool sucess = new PageScraper(page).Scrape();
            AddSuccessError(sucess);
        }

        private void AddToPendingPages(Page page)
        {
            lock (SyncPendingPages)
            {
                PendingPages.Add(page);
            }
        }

        private void AddToBlocker(Page page)
        {
            lock (SyncTempBlocker)
            {
                IpHostBlocker.Add(page.Website);
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
