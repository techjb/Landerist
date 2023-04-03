using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class TempBlocker
    {
        private readonly Dictionary<string, DateTime> IpBlocker = new();

        private readonly Dictionary<string, DateTime> HostBlocker = new();

        private const int MinSecconds = 10;

        private const int MaxSecconds = 20;

        public bool IsBlocked(Website website)
        {
            return IsBlocked(IpBlocker, website.IpAddress) ||
                IsBlocked(HostBlocker, website.Host);
        }

        private bool IsBlocked(Dictionary<string, DateTime> keyValuePairs, string? key)
        {
            if (key != null && keyValuePairs.ContainsKey(key))
            {
                var blockUntil = keyValuePairs[key];
                return blockUntil > DateTime.Now;
            }
            return false;
        }

        public void Add(Website website)
        {
            var ipBlockUntil = CalculateBlockUntil();
            Add(IpBlocker, website.IpAddress, ipBlockUntil);

            var hostBlockUntil = CalculateBlockUntil(website);
            Add(HostBlocker, website.Host, hostBlockUntil);
        }

        private DateTime CalculateBlockUntil()
        {
            int secconds = RandomSecconds();
            return DateTime.Now.AddSeconds(secconds);
        }

        private DateTime CalculateBlockUntil(Website website)
        {
            int randomSecconds = RandomSecconds();
            long crawDelay = website.CrawlDelay();
            int secconds = Math.Max(randomSecconds, (int)crawDelay);
            return DateTime.Now.AddSeconds(secconds);
        }

        private int RandomSecconds()
        {
            return new Random().Next(MinSecconds, MaxSecconds); ;
        }

        private void Add(Dictionary<string, DateTime> keyValuePairs, string? key, DateTime blockUntil)
        {
            if (key == null || key.Trim().Equals(string.Empty))
            {
                return;
            }

            if (keyValuePairs.ContainsKey(key))
            {
                keyValuePairs[key] = blockUntil;
            }
            else
            {
                keyValuePairs.Add(key, blockUntil);
            }
        }

        public void Clean()
        {
            Clean(IpBlocker);
            Clean(HostBlocker);
        }

        private void Clean(Dictionary<string, DateTime> keyValuePairs)
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
    }
}
