using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];
        private static readonly Dictionary<string, int> DictionaryHosts = [];
        private static readonly Dictionary<string, int> DictionaryIps = [];

        public static List<Page> Select()
        {
            Pages.Clear();
            DictionaryHosts.Clear();
            DictionaryIps.Clear();
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
            var pages = Websites.Pages.GetUnknownPageType();
            AddPages(pages);
        }

        private static void AddNextUpdate()
        {
            if (Pages.Count >= Config.MAX_PAGES_PER_SCRAPE)
            {
                return;
            }
            var pages = Websites.Pages.GetPagesNextUpdatePast();
            AddPages(pages);
        }

        private static void AddPagesToFillScrape()
        {
            int pagesToFill = Config.MAX_PAGES_PER_SCRAPE - Pages.Count;
            if (pagesToFill <= 0)
            {
                return;
            }

            var pages = Websites.Pages.GetPagesNextUpdateFuture();
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
                AddPage(page);
            }
        }

        private static void AddPage(Page page)
        {
            var host = page.Website.Host;
            if (DictionaryHosts.TryGetValue(host, out int hostCounter))
            {
                if (hostCounter >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
                {
                    return;
                }
                DictionaryHosts[host] = hostCounter + 1;
            }
            else
            {
                DictionaryHosts[host] = 1;
            }
            var ipAddress = page.Website.IpAddress;
            if (!string.IsNullOrEmpty(ipAddress))
            {
                if (DictionaryIps.TryGetValue(ipAddress, out int ipCounter))
                {
                    if (ipCounter >= Config.MAX_PAGES_PER_IP_PER_SCRAPE)
                    {
                        return;
                    }
                    DictionaryIps[ipAddress] = ipCounter + 1;
                }
                else
                {
                    DictionaryIps[ipAddress] = 1;
                }
            }
            Pages.Add(page);
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
