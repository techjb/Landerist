using landerist_library.Parse.ListingParser.StructuredOutputs;
using landerist_library.Websites;
using System.Text.Json;

namespace landerist_library.Parse.ListingParser
{
    public class ParseListingResponse
    {
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseListing(Page page, string arguments)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            try
            {
                var parseListingTool = JsonSerializer.Deserialize<StructuredOutputEsParse>(arguments);
                if (parseListingTool != null)
                {
                    result.pageType = PageType.ListingButNotParsed;
                    result.listing = parseListingTool.ToListing(page);
                    if (result.listing != null)
                    {
                        result.pageType = PageType.Listing;
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ParseListingResponse ParseListing", page.Uri, exception);
            }
            return result;
        }
    }
}
