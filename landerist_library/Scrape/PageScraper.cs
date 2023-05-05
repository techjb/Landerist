using landerist_library.Database;
using landerist_library.Download;
using landerist_library.Index;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.LocationParser;
using landerist_library.Parse.MediaParser;
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
            if (!HtmInLanguage())
            {
                new LinkAlternateIndexer(Page).Insert();
                return;
            }
            InsertPages();
            GetListing();
        }

        private bool HtmInLanguage()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                var htmlNode = Page.HtmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var langAttr = htmlNode.Attributes["lang"];
                    if (langAttr != null)
                    {
                        var language = langAttr.Value.ToLower();
                        if (!string.IsNullOrEmpty(language))
                        {
                            return language.StartsWith(Page.Website.Language);
                        }                        
                    }
                }
            }
            return true;
        }

        private void InsertPages()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument == null || Page.Website == null)
            {
                return;
            }
            new HyperlinksIndexer(Page).Insert();
        }

        private void GetListing()
        {
            if (!Page.CanRequestListing())
            {
                return;
            }
            var listingParser = new ListingParser(Page).GetListing();
            Page.IsListing = listingParser.Item1;
            var listing = listingParser.Item2;
            if (listing != null)
            {
                new LocationParser(Page, listing).SetLocation();
                new MediaParser(Page).AddMedia(listing);
                ES_Listings.InsertUpdate(listing);
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
            var redirectUrl = HttpClientDownloader.GetRedirectUrl();
            if (redirectUrl != null)
            {
                new Indexer(Page).InsertUrl(redirectUrl);
            }
        }
    }
}
