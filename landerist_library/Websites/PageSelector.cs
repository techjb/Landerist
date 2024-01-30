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

            //Listings();
            NewPages();
            DownloadError();
            //MayBeListing();
        }

        private static void Listings()
        {
            var pages = landerist_library.Websites.Pages.GetPages(PageType.Listing, 3, 2);
            Pages.AddRange(pages);
        }

        private static void NewPages()
        {
            var pages = landerist_library.Websites.Pages.GetUnknownPageType();
            Pages.AddRange(pages);
        }

        private static void DownloadError()
        {
            var pages = landerist_library.Websites.Pages.GetPages(PageType.DownloadError, 3, 5);
            Pages.AddRange(pages);
        }

        private static void MayBeListing()
        {
            var pages = landerist_library.Websites.Pages.GetPages(PageType.MayBeListing, 1);
            Pages.AddRange(pages);
        }

        private static void FilterPages()
        {
            Console.WriteLine("Filtering pages ..");
            FilterMaxPagesPerHost();
            FilterMaxTotalPages();
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
                    pages.Add(page);
                    dictionary.Add(host, 1);
                }
                else
                {
                    if (value < Configuration.Config.MAX_PAGES_PER_HOSTS_PER_SCRAPE)
                    {
                        pages.Add(page);
                        dictionary[host] += 1;
                    }
                }
            }

            Pages = pages;
        }

        private static void FilterMaxTotalPages()
        {
            if (Pages.Count > Configuration.Config.MAX_TOTAL_PAGES_PER_SCRAPE)
            {
                Pages = [.. Pages.AsParallel().OrderBy(o => o.Updated)];
                Pages = Pages.Take(Configuration.Config.MAX_TOTAL_PAGES_PER_SCRAPE).ToList();
            }
        }
    }
}
