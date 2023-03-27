using landerist_library.Websites;

namespace landerist_library.Scraper
{
    public class Scraper : ScraperBase
    {
        private readonly TempBlocker TempBlocker = new();

        private readonly object SyncBlocker = new();

        private readonly Dictionary<string, Website> DictionaryWebsites = new();

        private readonly List<Page> PendingPages = new();

        private int Counter = 0;

        private int Suceess = 0;

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

        public void AllPages()
        {
            var pages = new Pages().GetAll();
            TotalPages = pages.Count;
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
                    "Success: " + Suceess + " Errors: " + Errors);
            });

            if (PendingPages.Count > 0)
            {
                Thread.Sleep(2000);
                ScrapePages(PendingPages);
            }
        }

        private void ScrapePage(Page page)
        {
            IncreaseCounter();            
            if (!DictionaryWebsites.ContainsKey(page.Host))
            {
                return;
            }

            var website = DictionaryWebsites[page.Host];
            if (!website.CanAccess(page.Uri))
            {
                return;
            }

            if (TempBlocker.IsBlocked(website))
            {
                PendingPages.Add(page);
                return;
            }           

            AddToBlocker(website);
            bool sucess = page.Process(website);
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
                    Suceess++;
                }
                else
                {
                    Errors++;
                }
            }
        }
    }
}
