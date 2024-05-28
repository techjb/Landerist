using landerist_library.Websites;
using System.Text.Json;

namespace landerist_library.Parse.Listing
{
    public class ParseListingResponse
    {
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseListing(Page page, string arguments)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            try
            {
                var parseListingFunction = JsonSerializer.Deserialize<ParseListingTool>(arguments);
                if (parseListingFunction != null)
                {
                    result.pageType = PageType.ListingButNotParsed;
                    result.listing = parseListingFunction.ToListing(page);
                    if (result.listing != null)
                    {
                        result.pageType = PageType.Listing;
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ParseListingResponse ParseListing", page.Uri, exception);
            }
            return result;
        }
    }
}
