using landerist_library.Websites;
using System.Data;

namespace landerist_library.Scrape
{
    public class Scraper
    {
        private readonly TempBlocker TempBlocker = new();

        private readonly object SyncBlocker = new();

        private readonly List<Page> PendingPages = new();

        private int Counter = 0;

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
            var dataTable = new Pages().GetAll();
            var pages = GetPages(dictionary, dataTable);            
            ScrapePages(pages);
        }

        private List<Page> GetPages(Dictionary<string, Website> dictionary, DataTable dataTable)
        {
            List<Page> pages = new();
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

        public void ScrapeNonScrapped(Uri uri)
        {
            ScrapeNonScrapped(uri, false);
        }

        public void ScrapeNonScrapped(Uri uri, bool recursive)
        {
            Website website = new(uri);
            ScrapeNonScrapped(website, recursive);
        }
        public void ScrapeNonScrapped(Website website, bool recursive)
        {
            var pages = website.GetNonScrapedPages();
            if(pages.Count == 0)
            {
                return;
            }
            ScrapePages(pages);
            if (recursive)
            {
                ScrapeNonScrapped(website, recursive);
            }            
        }

        public void ScrapeUnknowIsListing(Uri uri)
        {
            Website website = new(uri);
            ScrapeUnknowIsListing(website);
        }

        public void ScrapeUnknowIsListing(Website website)
        {
            var pages = website.GetUnknowIsListingPages();
            ScrapePages(pages);
        }

        private void ScrapePages(List<Page> pages)
        {
            PendingPages.Clear();
            int TotalPages = pages.Count;
            Counter = 0;
            Parallel.ForEach(pages,
                new ParallelOptions() { MaxDegreeOfParallelism = 1 },
                page =>
                {
                    ScrapePage(page);
                    Console.WriteLine(
                        "Scrapped: " + Counter + "/" + TotalPages + " Pending: " + PendingPages.Count + " " +
                        "Success: " + Sucess + " Errors: " + Errors);
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
            IncreaseCounter();
            if (!page.CanScrape())
            {
                return;
            }

            if (TempBlocker.IsBlocked(page.Website))
            {
                PendingPages.Add(page);
                return;
            }
            AddToBlocker(page);
            bool sucess = page.Scrape();
            AddSuccessError(sucess);
        }

        private void IncreaseCounter()
        {
            lock (SyncCounter)
            {
                Counter++;
            }
        }

        private void AddToBlocker(Page page)
        {
            lock (SyncBlocker)
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
