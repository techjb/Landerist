using landerist_library.Database;
using landerist_orels.ES;

namespace landerist_tests
{
    internal static class ListingsTests
    {
        public static void Run()
        {
            //var page = new Page("https://www.nerjasolproperty.com/es/apartamento-en-venta-en-torrox-costa/80124/s2");
            //var listing1 = page.GetListing(true, true);

            //var source = new landerist_orels.Source()
            //{
            //    sourceName = "www.nerjasolproperty.com",
            //    sourceUrl = new Uri("https://www.nerjasolproperty.com/es/apartamento-en-venta-en-torrox-costa/80124/s2"),
            //    sourceGuid = "4196"
            //};
            //listing1.AddSource(source);
            //ES_Listings.InsertUpdate(page.Website, listing1);
            //ES_Sources.FixListingsWhitoutSource();

            //var listing = ES_Listings.GetListing("82BF926CB7C6AEF19A6F4CBCB81B16612F0F372D70173A83A24CB154D8CDAA52", true, true);
            //var sordedSet = new SortedSet<Listing> { listing };
            //var json = landerist_library.Export.Json.ExportListings(sordedSet, "C:\\Users\\Chus\\Downloads\\test.json");
            //Console.WriteLine(json);

            //ListingsCleanner.UnpublishListingsWithoutPage();
            //ListingsCleanner.UnpublishListingsByPageType("www.redpiso.es", landerist_library.Pages.PageType.NotIndexable);
        }
    }
}
