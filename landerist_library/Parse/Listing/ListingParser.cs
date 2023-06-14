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
            return
                !string.IsNullOrEmpty(Page.ResponseBodyText) &&
                ChatGPT.IsLengthAllowed(Page.ResponseBodyText);
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
                    if (Listing != null)
                    {
                        IsListing = Configuration.Config.TRAINING_MODE || Listing.IsValid();
                    }
                }
            }
            catch //(Exception exception)
            {
                //Logs.Log.WriteLogErrors(Page.Uri, exception);
            }
        }
    }
}
