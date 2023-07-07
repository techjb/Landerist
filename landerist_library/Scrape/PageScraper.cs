using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Download;
using landerist_library.Index;
using landerist_library.Parse.Listing;
using landerist_library.Parse.Location;
using landerist_library.Parse.Media;
using landerist_library.Websites;

namespace landerist_library.Scrape
{
    public class PageScraper
    {
        private readonly Page Page;

        private readonly HttpClientDownloader HttpClientDownloader;

        public PageScraper(Page page)
        {
            Page = page;
            HttpClientDownloader = new HttpClientDownloader();
        }

        public bool Scrape()
        {
            if (HttpClientDownloader.Get(Page))
            {
                DownloadSucess();
            }
            else
            {
                DownloadError();
            }

            return Page.Update();
        }

        private void DownloadSucess()
        {
            if (!HtmInWebsiteLanguage())
            {
                if (Config.INDEXER_ENABLED)
                {
                    new LinkAlternateIndexer(Page).InsertLinksAlternate();
                }
                return;
            }
            if (Page.CanFollowLinks())
            {
                IndexPages();
            }
            if (Page.CanIndexContent())
            {
                GetListing();
            }
        }

        private bool HtmInWebsiteLanguage()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                var htmlNode = Page.HtmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return LanguageValidator.IsValidLanguageAndCountry(Page.Website, lang.Value);
                    }
                }
            }
            return true;
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument == null || Page.Website == null)
            {
                return;
            }
            new HyperlinksIndexer(Page).Insert();
        }

        private void GetListing()
        {
            if (!IsListingParser.IsListing(Page))
            {
                return;
            }

            landerist_orels.ES.Listing? listing;            
            if (Config.LISTING_PARSER_ENABLED)
            {
                var listingParser = new ListingParser(Page).GetListing();
                Page.IsListing = listingParser.Item1;
                listing = listingParser.Item2;
            }
            else
            {
                listing = ES_Listings.GetListing(Page, false);
                Page.IsListing = listing != null;                
            }
            if (listing == null)
            {
                return;
            }

            if (Config.SET_LATLNG_LAUID_AND_MEDIA_TO_LISTING)
            {
                new LatLngParser(Page, listing).SetLatLng();
                new LauIdParser(Page, listing).SetLauId();
                new MediaParser(Page).AddMedia(listing);
                ES_Listings.InsertUpdate(listing);
            }
            else
            {
                ES_Listings.Insert(listing);
            }
        }

        private void DownloadError()
        {
            if (Page.HttpStatusCode == null)
            {
                return;
            }
            int code = (int)Page.HttpStatusCode;
            if (code >= 300 && code < 400)
            {
                DownloadErrorRedirect();
            };
        }

        private void DownloadErrorRedirect()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            var redirectUrl = HttpClientDownloader.GetRedirectUrl();
            if (redirectUrl != null)
            {
                new Indexer(Page).InsertUrl(redirectUrl);
            }
        }
    }
}
