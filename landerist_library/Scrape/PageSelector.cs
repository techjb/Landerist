using landerist_library.Configuration;
using landerist_library.Pages;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];

        public static List<Page> Select()
        {
            //Console.WriteLine("Selecting pages");
            Init();
            SelectPages();
            FilterMinPages();
            return [.. Pages];
        }

        private static void Init()
        {
            Pages.Clear();
            landerist_library.Pages.Pages.CleanLockedBy();
        }

        private static void SelectPages()
        {
            AddScrapePages();
            //AddRecentlyUnpublishedListingsPages();
        }

        private static void AddScrapePages()
        {
            var topRows = GetTopRows();
            var pages = landerist_library.Pages.Pages.GetScrapePages(topRows);
            AddPages(pages);
        }

        private static void AddUnknownPageType()
        {
            var topRows = GetTopRows();
            var pages = landerist_library.Pages.Pages.GetUnknownPageType(topRows);
            AddPages(pages);
        }

        private static void AddNextScrape()
        {
            if (ScraperIsFull())
            {
                return;
            }

            var topRows = GetTopRows();
            var pages = landerist_library.Pages.Pages.GetNextScrape(topRows, false);
            AddPages(pages);
        }

        private static void AddRecentlyUnpublishedListingsPages()
        {
            if (ScraperIsFull())
            {
                return;
            }

            var topRows = GetTopRows();
            var pages = landerist_library.Pages.Pages.GetRecentlyUnpublishedListingsPages(topRows);
            AddPages(pages);
        }

        private static int GetTopRows()
        {
            return Config.MAX_PAGES_PER_SCRAPE - Pages.Count;
        }

        private static List<Page> RemoveDuplicates(List<Page> pages)
        {
            var pageUriHashes = new HashSet<string>(Pages.Select(p => p.UriHash));
            return pages.AsParallel().Where(p => !pageUriHashes.Contains(p.UriHash)).ToList();
        }

        private static void AddPages(List<Page> pages)
        {
            pages = RemoveDuplicates(pages);
            foreach (var page in pages)
            {
                if (ScraperIsFull())
                {
                    return;
                }

                AddPage(page);
            }
        }

        private static bool AddPage(Page page)
        {
            if (Pages.Count(p => p.Website.Host.Equals(page.Website.Host)) >= Config.MAX_PAGES_PER_HOST_PER_SCRAPE)
            {
                return false;
            }

            Pages.Add(page);
            return true;
        }

        private static void FilterMinPages()
        {
            if (!Config.IsConfigurationProduction())
            {
                return;
            }

            if (Pages.Count < Config.MIN_PAGES_PER_SCRAPE)
            {
                Console.WriteLine("Not enough pages to scrape. " + Pages.Count);
                Pages.Clear();
            }
        }

        private static bool ScraperIsFull()
        {
            return Pages.Count >= Config.MAX_PAGES_PER_SCRAPE;
        }
    }
}
