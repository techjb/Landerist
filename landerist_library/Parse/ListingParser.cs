using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;

namespace landerist_library.Parse
{
    public class ListingParser
    {
        private readonly Page Page;

        private bool? IsListing = null;

        private Listing? Listing = null;

        public ListingParser(Page page)
        {
            Page = page;
        }

        public Tuple<bool?, Listing?> GetListing()
        {
            SetResponseBodyText();
            if (RequestListingIsPermited())
            {
                RequestListing();
            }
            return Tuple.Create(IsListing, Listing);
        }

        public bool RequestListingIsPermited()
        {
            return
                !string.IsNullOrEmpty(Page.ResponseBodyText) &&
                ChatGPT.IsLengthAllowed(Page.ResponseBodyText);
        }

        private void SetResponseBodyText()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                Page.ResponseBodyText = new HtmlToText(Page.HtmlDocument).GetText();
            }
        }

        private void RequestListing()
        {
            if (!string.IsNullOrEmpty(Page.ResponseBodyText))
            {
                var result = new ChatGPT().GetResponse(Page.ResponseBodyText);
                if (result != null)
                {
                    ParseListing(result);
                }
            }
        }

        private void ParseListing(string json)
        {
            IsListing = false;
            try
            {
                ListingResponse? listingResponse = JsonConvert.DeserializeObject<ListingResponse>(json);
                if (listingResponse != null)
                {
                    Listing = listingResponse.ToListing(Page);
                    IsListing = Listing != null;
                }
            }
            catch
            {
                
            }   
        }
    }
}
