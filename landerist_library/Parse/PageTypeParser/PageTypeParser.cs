﻿using landerist_library.Parse.ListingParser;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class PageTypeParser
    {
        private Page Page { get; }
        public PageTypeParser(Page page)
        {
            Page = page;
        }

        public (PageType? pageType, landerist_orels.ES.Listing? listing, bool waitingAIRequest)
            GetPageType()
        {
            if (Page == null)
            {
                return (PageType.DownloadError, null, false);
            }
            if (Page.ResponseBodyIsNullOrEmpty())
            {
                //Console.WriteLine("ResponseBodyIsNullOrEmpty in GetPageType " + Page.Uri);
                return (PageType.DownloadError, null, false);
            }
            if (Page.HttpStatusCode != 200)
            {
                //Console.WriteLine("HttpStatusCode is not 200 in GetPageType: " + Page.HttpStatusCode);
                return (PageType.DownloadError, null, false);
            }
            if (Page.MainPage())
            {
                return (PageType.MainPage, null, false);
            }
            if (Page.ContainsMetaRobotsNoIndex())
            {
                return (PageType.NotIndexable, null, false);
            }
            if (Page.NotCanonical())
            {
                return (PageType.NotCanonical, null, false);
            }            
            if (Page.IncorrectLanguage())
            {
                return (PageType.IncorrectLanguage, null, false);
            }

            Page.SetResponseBodyText();

            if (Page.ResponseBodyTextHasNotChanged())
            {
                return (Page.PageType, null, false);
            }
            if (Page.ResponseBodyTextIsError())
            {
                return (PageType.ResponseBodyIsError, null, false);
            }
            if (Page.ResponseBodyTextIsTooShort())
            {
                return (PageType.ResponseBodyTooShort, null, false);
            }
            if (Page.ResponseBodyTextIsTooLarge())
            {
                return (PageType.ResponseBodyTooLarge, null, false);
            }
            if (Page.ReponseBodyTextRepeatedInHost())
            {
                return (PageType.ResponseBodyRepeatedInHost, null, false);
            }
            if (Page.ReponseBodyTextRepeatedInListings())
            {
                return (PageType.ResponseBodyRepeatedInListings, null, false);
            }
            if (ParseListing.TooManyTokens(Page))
            {
                return (PageType.ResponseBodyTooManyTokens, null, false);
            }
            return ParseListing.Parse(Page);
        }
    }
}
