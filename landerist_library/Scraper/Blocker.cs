﻿using landerist_library.Websites;

namespace landerist_library.Scraper
{
    public class Blocker
    {
        private readonly Dictionary<string, DateTime> IpBlocker = new ();

        private readonly Dictionary<string, DateTime> HostBlocker = new ();

        public const int BlockSecconds = 10;

        public bool CanScrape(Website website)
        {
            bool isIpBlocked = IsBlocked(IpBlocker, website.IpAddress);
            if (isIpBlocked)
            {
                return false;
            }
            return !IsBlocked(HostBlocker, website.Host);
        }

        private bool IsBlocked(Dictionary<string, DateTime> keyValuePairs, string? key)
        {
            if (key != null && keyValuePairs.ContainsKey(key))
            {
                var value = keyValuePairs[key];
                return value > DateTime.Now.AddSeconds(-BlockSecconds);
            }
            return false;
        }

        public void Add(Website website)
        {
            Add(IpBlocker, website.IpAddress);
            Add(HostBlocker, website.Host);
        }

        private void Add(Dictionary<string, DateTime> keyValuePairs, string? key)
        {
            if(key == null)
            {
                return;
            }

            if(keyValuePairs.ContainsKey(key))
            {
                keyValuePairs[key] = DateTime.Now;
            }
            else
            {
                keyValuePairs.Add(key, DateTime.Now);
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
                if (keyValuePairs[key] < DateTime.Now.AddSeconds(-BlockSecconds))
                {
                    keyValuePairs.Remove(key);
                }
            }
        }
    }
}
