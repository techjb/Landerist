﻿using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse
{
    public class ListingParser
    {
        private readonly Page Page;

        private bool? IsListing = null;

        private Listing? Listing = null;

        private string ResponseBodyText = string.Empty;

        public ListingParser(Page page)
        {
            Page = page;
        }

        public Tuple<bool?,Listing?> GetListing()
        {
            SetResponseBodyText();
            if (CanRequestListing())
            {
                RequestListing();
            }
            return new Tuple<bool?, Listing?>(IsListing, Listing);
        }

        public bool CanRequestListing()
        {
            return 
                !ResponseBodyText.Equals(string.Empty) &&
                ResponseBodyText.Length < ChatGPT.MAX_REQUEST_LENGTH &&
                !Page.IsMainPage();
        }

        private void SetResponseBodyText()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                ResponseBodyText = new HtmlToText(Page.HtmlDocument).GetText();
            }
        }

        private void RequestListing()
        {
            var task = Task.Run(async () => await new ChatGPT().GetListing(ResponseBodyText));
            if (task.Result != null)
            {
                if (task.Result.Equals("no"))
                {
                    IsListing = false;
                }
                else
                {
                    // parse
                    IsListing = true;
                }
            }
        }
    }
}