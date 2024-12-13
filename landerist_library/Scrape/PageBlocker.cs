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

            var blockedTimeHost = GetBlockedTime(HostBlocker, page.Website.Host);
            var blockedTimeIp = GetBlockedTime(IpBlocker, page.Website.IpAddress);

            var blockedTime = blockedTimeHost > blockedTimeIp ? blockedTimeHost : blockedTimeIp;
            if (blockedTime > DateTime.Now)
            {
                Pages.Add((page, blockedTime));
                return true;
            }
            return false;
        }

        private static DateTime GetBlockedTime(Dictionary<string, DateTime> blocker, string? key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return DateTime.MinValue;
            }
            if (blocker.TryGetValue(key, out DateTime blockUntil))
            {
                return blockUntil;
            }
            return DateTime.MinValue;
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
            var ipBlockUntil = CalculateIpBlockUntil();
            Add(IpBlocker, website.IpAddress, ipBlockUntil);

            var hostBlockUntil = CalculateHostBlockUntil(website);
            Add(HostBlocker, website.Host, hostBlockUntil);
        }

        private static DateTime CalculateIpBlockUntil()
        {
            int secconds = RandomSecconds();
            return DateTime.Now.AddSeconds(secconds);
        }

        private static DateTime CalculateHostBlockUntil(Website website)
        {
            int randomSecconds = RandomSecconds();
            int crawDelay = website.CrawlDelay();
            int secconds = Math.Max(randomSecconds, crawDelay);
            return DateTime.Now.AddSeconds(secconds);
        }

        private static int RandomSecconds()
        {
            return new Random().Next(MinSecconds, MaxSecconds);
        }

        private static void Add(Dictionary<string, DateTime> keyValuePairs, string? key, DateTime blockUntil)
        {
            if (string.IsNullOrEmpty(key))
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
            foreach(var page in Pages)
            {
                page.page.Dispose();
            }
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
