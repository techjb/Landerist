using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using Newtonsoft.Json;

namespace landerist_library.Parse.Listing
{
    public class ListingParser
    {
        private readonly Page Page;

        private bool? IsListing = null;

        private landerist_orels.ES.Listing? Listing = null;

        public ListingParser(Page page)
        {
            Page = page;
        }

        public Tuple<bool?, landerist_orels.ES.Listing?> GetListing()
        {
            Page.SetResponseBodyText();
            if (RequestListingIsPermited())
            {
                RequestListing();
            }
            return Tuple.Create(IsListing, Listing);
        }

        public bool RequestListingIsPermited()
        {
            if (string.IsNullOrEmpty(Page.ResponseBodyText))
            {
                return false;
            }
            if (ResponseBodyTextIsError())
            {
                return false;
            }
            if (Configuration.Config.LISTINGS_PARSER_ENABLED && 
                !ChatGPTRequest.IsLengthAllowed(Page.ResponseBodyText))
            {
                return false;
            }
            return true;
        }

        private bool ResponseBodyTextIsError()
        {
            if (Page.ResponseBodyText == null)
            {
                return false;
            }
            return
                Page.ResponseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                Page.ResponseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                Page.ResponseBodyText.Contains("Página no encontrada", StringComparison.OrdinalIgnoreCase) ||
                Page.ResponseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }

        private void RequestListing()
        {
            if (string.IsNullOrEmpty(Page.ResponseBodyText))
            {
                return;
            }

            var result = new ChatGPTRequest().GetResponse(Page.ResponseBodyText);
            if (!string.IsNullOrEmpty(result))
            {
                ParseListing(result);
            }
        }

        private void ParseListing(string json)
        {
            try
            {
                ChatGPTResponse? listingResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(json);
                if (listingResponse != null)
                {
                    Listing = listingResponse.ToListing(Page);
                }
            }
            catch //(Exception exception)
            {
                //Logs.Log.WriteLogErrors(Page.Uri, exception);
            }
            IsListing = Listing != null;
        }
    }
}
