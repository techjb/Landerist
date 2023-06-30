using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;

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
                //RequestListing();
                RequestIsListing();
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
            if (Configuration.Config.CHATGPT_ENABLED &&
                //!ChatGPTRequest.IsLengthAllowed(ChatGPTGetListing.SystemMessage, Page.ResponseBodyText)
                !ChatGPTIsListing.IsTextAllowed(Page.ResponseBodyText)
                )
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
            Listing = new ChatGPTGetListing().GetListing(Page);
            IsListing = Listing != null;
        }

        private void RequestIsListing()
        {
            if (!string.IsNullOrEmpty(Page.ResponseBodyText))
            {
                IsListing = new ChatGPTIsListing().IsListing(Page.ResponseBodyText);
            }
        }
    }
}
