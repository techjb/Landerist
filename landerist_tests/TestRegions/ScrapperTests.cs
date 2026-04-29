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
            //new Scraper().ProcessPages();
            //new Scraper().Scrape(page, false);

            new landerist_library.Scrape.Scraper().Scrape("https://www.remax.es/buscador-de-inmuebles/venta/chalet-pareado/madrid/las-rozas-de-madrid/todos/3367-04513/", false);
            //new Scraper().TestSinglePage();
            //landerist_library.Scrape.PageSelector.SelectTop1();
            //Console.WriteLine("Block: " + WebsitesThrottle.Block(page.Website));
            //Console.WriteLine("IsBlocked: " + WebsitesThrottle.IsBlocked(page.Website));
            //new landerist_library.Scrape.Scraper().Start();
        }
    }
}
