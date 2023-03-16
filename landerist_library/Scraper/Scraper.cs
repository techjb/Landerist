using landerist_library.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Scraper
{
    public class Scraper : ScraperBase
    {
        private readonly Blocker Blocker = new();

        private readonly object SyncBlocker = new();

        private readonly Dictionary<string, Website> DictionaryWebsites = new();

        private List<Page> PendingPages = new();

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
                //new ParallelOptions() { MaxDegreeOfParallelism = 1 }, 
                page =>
            {
                bool success = ScrapePage(page);
                lock (SyncSuccessErrros)
                {
                    if(success)
                    {
                        Suceess++;
                    }
                    else
                    {
                        Errors++;
                    }
                }
                Console.WriteLine("Scrapped: " + Counter + "/" + TotalPages + " Pending: " + PendingPages.Count + " " +
                    "Success: " + Suceess + " Errors: " + Errors);
            });

            if (PendingPages.Count > 0)
            {
                Thread.Sleep(Blocker.BlockSecconds * 1000);
                ScrapePages(PendingPages);
            }
        }

        private bool ScrapePage(Page page)
        {
            if (!DictionaryWebsites.ContainsKey(page.Host))
            {
                return false;
            }
            var website = DictionaryWebsites[page.Host];

            if (!Blocker.CanScrape(website))
            {
                PendingPages.Add(page);
                return false;
            }
            lock (SyncBlocker)
            {
                Blocker.Add(website);
            }
            lock (SyncCounter)
            {
                Counter++;
            }

            return page.SetBodyAndStatusCode();
        }
    }
}
