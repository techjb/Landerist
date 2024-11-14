using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static List<Page> Pages = [];

        public static List<Page> Select()
        {
            Pages.Clear();
            SelectPages();
            FilterPages();
            return Pages;
        }

        private static void SelectPages()
        {
            Console.WriteLine("Selecting pages ..");

            AddUnknowPageType();
            AddNextUpdate();
            AddPagesToFillScrape();
        }

        private static void AddUnknowPageType()
        {
            var pages = Websites.Pages.GetUnknownPageType(Config.MAX_PAGES_PER_SCRAPE);
            Pages.AddRange(pages);
        }

        private static void AddNextUpdate()
        {
            var pages = Websites.Pages.GetPagesNextUpdatePast(Config.MAX_PAGES_PER_SCRAPE);
            Pages.AddRange(pages);
        }

        private static void AddPagesToFillScrape()
        {
            int pagesToFill = Config.MAX_PAGES_PER_SCRAPE - Pages.Count;
            if (pagesToFill <= 0)
            {
                return;
            }

            var pages = Websites.Pages.GetPagesNextUpdateFuture(pagesToFill);
            pages = pages.Where(p => !Pages.Any(p2 => p2.UriHash == p.UriHash)).ToList();
            Pages.AddRange(pages);
        }

        private static void FilterPages()
        {
            Console.WriteLine("Filtering pages ..");
            FilterMaxPagesHostsPerScrape();
            FilterMaxTotalPages();
            FilterMinTotalPages();
        }

        private static void FilterMaxPagesHostsPerScrape()
        {
            List<Page> pages = [];
            Dictionary<string, int> dictionary = [];
            foreach (var page in Pages)
            {
                var host = page.Website.Host;
                if (dictionary.TryGetValue(host, out int value))
                {
                    if (value >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
                    {
                        continue;
                    }
                    dictionary[host] = value + 1;
                }
                else
                {
                    dictionary[host] = 1;
                }
                pages.Add(page);
            }
            Pages = pages;
        }

        private static void FilterMaxTotalPages()
        {
            if (Pages.Count > Config.MAX_PAGES_PER_SCRAPE)
            {
                Pages = [.. Pages.AsParallel().OrderBy(o => o.Updated)];
                Pages = Pages.Take(Config.MAX_PAGES_PER_SCRAPE).ToList();
            }
        }

        private static void FilterMinTotalPages()
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
