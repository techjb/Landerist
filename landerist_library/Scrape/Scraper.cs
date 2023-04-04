using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class Scraper 
    {
        private readonly TempBlocker TempBlocker = new();

        private readonly object SyncBlocker = new();

        private readonly Dictionary<string, Website> DictionaryWebsites = new();

        private readonly List<Page> PendingPages = new();

        private int Counter = 0;

        private int Sucess = 0;

        private int Errors = 0;

        private int TotalPages = 0;

        private readonly object SyncCounter = new();

        private readonly object SyncSuccessErrros = new();

        public Scraper()
        {
            InitDictionary();
        }

        private void InitDictionary()
        {
            var websites = Websites.Websites.GetStatusCodeOk();
            foreach (var website in websites)
            {
                DictionaryWebsites.Add(website.Host, website);
            }
        }

        public void ScrapeAllPages()
        {
            var pages = new Pages().GetAll();
            TotalPages = pages.Count;
            ScrapePages(pages);
        }

        public void ScrapeMainPage(Website website)
        {
            var page = new Page(website, website.MainUri);
            ScrapePage(page);
        }

        public void ScrapePages(Website website)
        {
            var pages = website.GetPages();
            ScrapePages(pages);
        }

        private void ScrapePages(List<Page> pages)
        {
            PendingPages.Clear();

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
                ScrapePages(PendingPages);
            }
        }

        public void ScrapePage(Page page)
        {
            IncreaseCounter();            
            if (!DictionaryWebsites.ContainsKey(page.Host))
            {
                return;
            }

            var website = DictionaryWebsites[page.Host];
            if (!website.IsUriAllowed(page.Uri))
            {
                return;
            }

            if (TempBlocker.IsBlocked(website))
            {
                PendingPages.Add(page);
                return;
            }           

            AddToBlocker(website);            
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

        private void AddToBlocker(Website website)
        {
            lock (SyncBlocker)
            {
                TempBlocker.Add(website);
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
