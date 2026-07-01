using landerist_library.Scrape;

namespace landerist_tests
{
    internal static class ScrapperTests
    {
        public static void Run()
        {
            //new Scraper(false).ScrapeUnknowPageType(10000);
            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapeUnknowHttpStatusCode();
            //new Scraper(true).ScrapeUnknowIsListing(uri);
            //new Scraper().ScrapeIsNotListing(uri);
            //new Scraper().Scrape(website);
            //new Scraper().ScrapeUnknowPageType(website);
            //new Scraper().ScrapeAllPages();
            //new Scraper().ScrapeResponseBodyRepeatedInListings();

            //new Scraper().Start();
            //new Scraper().Scrape(page, false);

            new landerist_library.Scrape.Scraper().Scrape("https://realestate.hipoges.com/es/detail/GTRE-00025", false);
            //new landerist_library.Scrape.Scraper().TryApplyPreClassificationBeforeDownload(new landerist_library.Pages.Page("https://www.iadespana.es/anuncios/aspe-03680/venta/casa"));
            //new Scraper().TestSinglePage();
            //landerist_library.Scrape.PageSelector.SelectTop1();
            //Console.WriteLine("Block: " + WebsitesThrottle.Block(page.Website));
            //Console.WriteLine("IsBlocked: " + WebsitesThrottle.IsBlocked(page.Website));
            //new landerist_library.Scrape.Scraper().Start();
        }
    }
}
