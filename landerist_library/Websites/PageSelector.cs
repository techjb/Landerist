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
            if (Pages.Count > Configuration.Config.PAGES_PER_SCRAPE)
            {
                Console.WriteLine("Filtering pages ..");
                Pages = [.. Pages.AsParallel().OrderBy(o => o.Updated)];
                Pages = Pages.Take(Configuration.Config.PAGES_PER_SCRAPE).ToList();
            }
        }
    }
}
