﻿using landerist_library.Parse.Listing.ChatGPT;
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
                ChatGPTRequest.IsLengthAllowed(Page.ResponseBodyText);
        }

        private void RequestListing()
        {
            if (!string.IsNullOrEmpty(Page.ResponseBodyText))
            {
                var result = new ChatGPTRequest().GetResponse(Page.ResponseBodyText);
                if (!string.IsNullOrEmpty(result))
                {
                    ParseListing(result);
                }
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
