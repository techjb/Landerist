using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];
        //private static readonly Dictionary<string, int> DictionaryHosts = [];
        //private static readonly Dictionary<string, int> DictionaryIps = [];

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
            //DictionaryHosts.Clear();
            //DictionaryIps.Clear();
        }

        private static void SelectPages()
        {
            AddUnknowPageType();
            AddNextUpdate();
            AddPagesToFillScrape();
            //FilterMinPages();
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
            //var (hosts, ips) = GetBlockedHostsAndIps();
            var pages = Websites.Pages.GetNextUpdate(topRows);
            AddPages(pages);
        }

        private static int GetTopRows()
        {
            return Config.MAX_PAGES_PER_SCRAPE - Pages.Count;
        }

        private static void AddPagesToFillScrape()
        {
            if (ScrapperIsFull())
            {
                return;
            }
            var topRows = GetTopRows();
            //var (hosts, ips) = GetBlockedHostsAndIps();
            var pages = Websites.Pages.GetNextUpdateFuture(topRows);
            AddPages(pages);
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
            //var host = page.Website.Host;
            //if (DictionaryHosts.TryGetValue(host, out int hostCounter))
            //{
            //    if (hostCounter >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
            //    {
            //        return false;
            //    }
            //    DictionaryHosts[host] = hostCounter + 1;
            //}
            //else
            //{
            //    DictionaryHosts[host] = 1;
            //}
            //var ipAddress = page.Website.IpAddress;
            //if (!string.IsNullOrEmpty(ipAddress))
            //{
            //    if (DictionaryIps.TryGetValue(ipAddress, out int ipCounter))
            //    {
            //        if (ipCounter >= Config.MAX_PAGES_PER_IP_PER_SCRAPE)
            //        {
            //            return false;
            //        }
            //        DictionaryIps[ipAddress] = ipCounter + 1;
            //    }
            //    else
            //    {
            //        DictionaryIps[ipAddress] = 1;
            //    }
            //}
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

        //private static (List<string> hosts, List<string> ips) GetBlockedHostsAndIps()
        //{
        //    var hosts = DictionaryHosts
        //        .Where(o => o.Value >= Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
        //        .Select(o => o.Key).ToList();

        //    var ips = DictionaryIps
        //        .Where(o => o.Value >= Config.MAX_PAGES_PER_IP_PER_SCRAPE)
        //        .Select(o => o.Key).ToList();

        //    return (hosts, ips);
        //}
    }
}
