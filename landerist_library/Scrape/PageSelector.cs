using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];
        private static readonly Dictionary<string, int> Dictionary = [];
        private static readonly int SELECT_COUNTER = Config.MAX_PAGES_PER_SCRAPE * 2;

        public static List<Page> Select()
        {
            Pages.Clear();
            Dictionary.Clear();
            SelectPages();
            return Pages;
        }

        private static void SelectPages()
        {
            AddUnknowPageType();
            AddNextUpdate();
            AddPagesToFillScrape();
            FilterMinPages();
        }

        private static void AddUnknowPageType()
        {
            var pages = Websites.Pages.GetUnknownPageType(SELECT_COUNTER);
            AddPages(pages);
        }

        private static void AddNextUpdate()
        {
            var pages = Websites.Pages.GetPagesNextUpdatePast(SELECT_COUNTER);
            AddPages(pages);
        }

        private static void AddPagesToFillScrape()
        {
            int pagesToFill = SELECT_COUNTER - Pages.Count;
            if (pagesToFill <= 0)
            {
                return;
            }

            var pages = Websites.Pages.GetPagesNextUpdateFuture(pagesToFill);
            pages = pages.Where(p1 => !Pages.Any(p2 => p2.UriHash == p1.UriHash)).ToList();
            AddPages(pages);
        }

        private static void AddPages(List<Page> pages)
        {
            foreach (var page in pages)
            {
                if (Pages.Count >= Config.MAX_PAGES_PER_SCRAPE)
                {
                    return;
                }
                var host = page.Website.Host;
                if (Dictionary.TryGetValue(host, out int value))
                {
                    if (value >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
                    {
                        continue;
                    }
                    Dictionary[host] = value + 1;
                }
                else
                {
                    Dictionary[host] = 1;
                }
                Pages.Add(page);
            }
        }

        

        private static void FilterMinPages()
        {
            if (!Config.IsConfigurationProduction())
            {
                return;
            }

            if (Pages.Count < Config.MIN_PAGES_PER_SCRAPE)
            {
                Pages.Clear();
            }
        }
    }
}
