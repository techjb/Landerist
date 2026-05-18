using landerist_library.Websites;

namespace landerist_tests
{
    internal static class WebsitesTests
    {
        public static void Run()
        {
            //var website = new Website(uri);
            //Websites.DeleteCurentMachineLogs(website); return;
            //Websites.DeleteCurentMachineLogs(uri); return;
            //Websites.DeleteAll(); return;

            //Websites.SetHttpStatusCodesToNull();
            //Websites.InsertUpdateUrisFromNotOk();
            //Websites.SetHttpStatusCodesToAll();
            //Websites.SetRobotsTxtToHttpStatusCodeOk();
            //Websites.SetIpAdress();
            //Websites.CountCanAccesToMainUri();
            //Websites.CountRobotsSiteMaps();
            //Websites.InsertMainPages();
            //Websites.UpdateNumPages();
            //Websites.SetPageTypeAndNextScrape();

            //Websites.DeleteFromFile();

            //var website = new Website("www.servihabitat.com");

            //if (website.SetRobotsTxt())
            //{
            //    website.Update();
            //}
            //website.ReadSitemap();

            //new Website("promoaguilera.com").DeleteCurentMachineLogs();
            //new Website("www.servihabitat.com").ReadSitemap();

            //WebsitesCleanner.DeleteWebsitesWithLessThanPages(100);
            //WebsitesCleanner.DeleteWebsitesWithoutListings();
            //WebsitesCleanner.DeleteWebsitesWithoutPageTypeListing();
            //WebsitesCleanner.DeleteWebsitesWithoutPublishedListings();
            //WebsitesCleanner.DeletePagesDiscardedByIndexUrlRegex("www.engelvoelkers.com");


            string hostUri = "www.coldwellbanker.es";
            //string mainUri = "https://century21.es/";
            //string listingUrlRegex = @"^https:\/\/century21\.es\/ref\/[A-Z0-9]+(?:-[A-Z0-9]+)+$";
            //string indexUrlRegex = listingUrlRegex;
            //string sitemapUrlRegex = @"^https:\/\/century21\.es\/(?:sitemap_index\.xml|ref\/sitemap\/\d+\.xml)$";
            //string allowebResourceTypes = "Unknown,Beacon,Document,StyleSheet,Script,TextTrack,Xhr,Fetch,EventSource,WebSocket,Manifest,Ping,Other";

            //WebsitesInserter.InsertSpecialWebsite(mainUri, hostUri, listingUrlRegex, indexUrlRegex, sitemapUrlRegex, allowebResourceTypes);

            Website website = new(hostUri);
            website.ReadSitemap();

        }
    }
}
