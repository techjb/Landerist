using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];    

        public static List<Page> Select()
        {
            Logs.Log.Console("Selecting pages");
            Init();
            SelectPages();
            return Pages;
        }

        private static void Init()
        {
            Pages.Clear();
            Websites.Pages.CleanLockedBy();            
        }

        private static void SelectPages()
        {
            AddUnknowPageType();
            AddNextUpdate();
        }

        private static void AddUnknowPageType()
        {
            var topRows = GetTopRows();
            var pages = Websites.Pages.GetUnknownPageType(topRows);
            AddPages(pages);
        }

        private static void AddNextUpdate()
        {
            if (ScrapperIsFull())
            {
                return;
            }
            var topRows = GetTopRows();
            var pages = Websites.Pages.GetNextUpdate(topRows, false);
            AddPages(pages);
        }

        private static void AddUnpublishedPages()
        {
            if (ScrapperIsFull())
            {
                return;
            }
            var topRows = GetTopRows();
            var pages = Websites.Pages.GetUnpublishedPages(topRows);
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
                if (ScrapperIsFull())
                {
                    return;
                }
                AddPage(page);
            }
        }

        private static bool AddPage(Page page)
        {
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

        private static bool ScrapperIsFull()
        {
            return Pages.Count >= Config.MAX_PAGES_PER_SCRAPE;
        }
    }
}
