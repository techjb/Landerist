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
            if (RequestListingIsPermited())
            {
                RequestListing();
                //RequestIsListing();
            }
            return Tuple.Create(IsListing, Listing);
        }

        public bool RequestListingIsPermited()
        {
            if (!IsListingParser.IsListing(Page))
            {
                return false;
            }
            if (//!ChatGPTRequest.IsLengthAllowed(ChatGPTGetListing.SystemMessage, Page.ResponseBodyText)
                !ChatGPTIsListing.IsTextAllowed(Page.ResponseBodyText)
                )
            {
                return false;
            }
            return true;
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
