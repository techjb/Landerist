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
            Pages.AddRange(pages);
        }

        private static void NewPages()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetUnknowPageType();
            Pages.AddRange(pages);
        }

        private static void DownloadError()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetPages(PageType.DownloadError, -3);
            Pages.AddRange(pages);
        }

        private static void MayBeListing()
        {
            if (!CanAddNewPages())
            {
                return;
            }

            var pages = landerist_library.Websites.Pages.GetPages(PageType.MayBeListing, -1);
            Pages.AddRange(pages);
        }

        private static bool CanAddNewPages()
        {
            return Pages.Count < Configuration.Config.SCRAPER_PAGES_BUNCH;
        }
    }
}
