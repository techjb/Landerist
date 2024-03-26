using landerist_library.Configuration;

namespace landerist_library.Websites
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
        }

        private static void AddUnknowPageType()
        {
            var pages = landerist_library.Websites.Pages.GetUnknownPageType();
            Pages.AddRange(pages);
        }

        private static void AddNextUpdate()
        {
            var pages = landerist_library.Websites.Pages.GetPagesNextUpdate();
            Pages.AddRange(pages);
        }

        private static void FilterPages()
        {
            Console.WriteLine("Filtering pages ..");
            FilterMaxPagesPerHost();
            FilterMaxTotalPages();
            FilterMinTotalPages();
        }

        private static void FilterMaxPagesPerHost()
        {
            List<Page> pages = [];
            Dictionary<string, int> dictionary = [];
            foreach (var page in Pages)
            {
                var host = page.Website.Host;
                if (!dictionary.TryGetValue(host, out int value))
                {
                    dictionary[host] = 1;
                }
                else
                {
                    if (value >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
                    {
                        continue;
                    }
                    dictionary[host] = value + 1;
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
