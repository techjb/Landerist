using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageBlocker
    {
        private readonly Dictionary<string, DateTime> IpBlocker = [];

        private readonly Dictionary<string, DateTime> HostBlocker = [];

        private const int MinSecconds = 3;

        private const int MaxSecconds = 6;

        private static readonly List<(Page page, DateTime blockUntil)> Pages = [];


        public bool IsBlocked(Page page)
        {
            if (page == null || page.Website == null)
            {
                return false;
            }

            var now = DateTime.Now;
            if (IsBlockedByIp(IpBlocker, page.Website.IpAddress, now, page) ||
                IsBlockedByIp(HostBlocker, page.Website.Host, now, page))
            {
                return true;
            }

            return false;
        }

        private static bool IsBlockedByIp(Dictionary<string, DateTime> blocker, string? key, DateTime now, Page page)
        {
            if (key != null && blocker.TryGetValue(key, out DateTime blockUntil) && blockUntil > now)
            {
                Pages.Add((page, blockUntil));
                return true;
            }
            return false;
        }


        public static List<Page> GetUnblockedPages()
        {
            var now = DateTime.Now;
            var unblockedPages = Pages.Where(o => o.blockUntil < now).ToList();
            if (unblockedPages.Count > 0)
            {
                Pages.RemoveAll(o => o.blockUntil < now);
            }
            return unblockedPages.Select(o => o.page).ToList();
        }

        public void Add(Website website)
        {
            var ipBlockUntil = CalculateBlockUntil();
            Add(IpBlocker, website.IpAddress, ipBlockUntil);

            var hostBlockUntil = CalculateBlockUntil(website);
            Add(HostBlocker, website.Host, hostBlockUntil);
        }

        private static DateTime CalculateBlockUntil()
        {
            int secconds = RandomSecconds();
            return DateTime.Now.AddSeconds(secconds);
        }

        private static DateTime CalculateBlockUntil(Website website)
        {
            int randomSecconds = RandomSecconds();
            int crawDelay = website.CrawlDelay();
            int secconds = Math.Max(randomSecconds, crawDelay);
            return DateTime.Now.AddSeconds(secconds);
        }

        private static int RandomSecconds()
        {
            //return 100;
            return new Random().Next(MinSecconds, MaxSecconds);
        }

        private static void Add(Dictionary<string, DateTime> keyValuePairs, string? key, DateTime blockUntil)
        {
            if (key == null || key.Trim().Equals(string.Empty))
            {
                return;
            }

            if (!keyValuePairs.TryAdd(key, blockUntil))
            {
                keyValuePairs[key] = blockUntil;
            }
        }

        public void Clean()
        {
            Clean(IpBlocker);
            Clean(HostBlocker);
            Pages.Clear();
        }

        private static void Clean(Dictionary<string, DateTime> keyValuePairs)
        {
            foreach (var key in keyValuePairs.Keys.ToList())
            {
                var blockUntil = keyValuePairs[key];
                if (blockUntil < DateTime.Now)
                {
                    keyValuePairs.Remove(key);
                }
            }
        }

        public static int CountBlockedPages()
        {
            return Pages.Count;
        }
    }
}
