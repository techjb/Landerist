namespace landerist_library.Websites
{
    public class PageSelector
    {
        private static readonly List<Page> Pages = [];

        public static List<Page> Select()
        {
            Pages.Clear();

            Listings();
            NewPages();
            DownloadError();
            MayBeListing();

            return Pages;
        }

        private static void Listings()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetPages(PageType.Listing, -3);
            AddPages(pages);
        }

        private static void NewPages()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetUnknownPageType();
            AddPages(pages);
        }

        private static void DownloadError()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetPages(PageType.DownloadError, -3);
            AddPages(pages);
        }

        private static void MayBeListing()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetPages(PageType.MayBeListing, -1);
            AddPages(pages);
        }

        private static bool CanAddNewPages()
        {
            return Pages.Count < Configuration.Config.PAGES_PER_SCRAPE;
        }

        private static void AddPages(List<Page> pages)
        {
            var remaining = Configuration.Config.PAGES_PER_SCRAPE - Pages.Count;
            if (remaining <= 0)
            {
                return;
            }
            if (pages.Count > remaining)
            {
                pages = pages.Take(remaining).ToList();
            }
            Pages.AddRange(pages);
        }
    }
}
