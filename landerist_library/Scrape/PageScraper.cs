using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Download;
using landerist_library.Index;
using landerist_library.Parse.Location;
using landerist_library.Parse.Media;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class PageScraper(Page page)
    {
        private readonly Page Page = page;

        private Listing? Listing;

        private readonly HttpClientDownloader HttpClientDownloader = new();

        public bool Scrape()
        {
            string? responseBody = HttpClientDownloader.Get(Page);
            Page.SetResponseBody(responseBody);

            SetPageType();
            UpdateIfListing();
            RedirectIfError();
            IndexPages();

            return Page.Update();
        }
        private void SetPageType()
        {
            Page.LoadHtmlDocument();
            var (newPageType, listing) = PageTypeParser.GetPageType(Page);
            Listing = listing;

            SetToUnpublished(newPageType);
            Page.SetPageType(newPageType);
        }

        private void SetToUnpublished(PageType? newPageType)
        {
            if (Page.PageType != null && Page.PageType.Equals(PageType.Listing))
            {
                if (newPageType != PageType.Listing)
                {
                    Listing ??= Page.GetListing(true);
                    if (Listing != null)
                    {
                        Listing.listingStatus = ListingStatus.unpublished;
                    }
                }
            }
        }

        private void UpdateIfListing()
        {
            if (Page.PageType.Equals(PageType.Listing))
            {
                Listing ??= Page.GetListing(true);
                SetMedia();
                SetLocation();
                UpdateListing();
            }
        }

        private void SetMedia()
        {
            if (Listing != null)
            {
                MediaParser mediaParser = new(Page);
                mediaParser.AddMedia(Listing);
            }
        }

        private void SetLocation()
        {
            if (Listing != null)
            {
                LatLngParser latLngParser = new(Page, Listing);
                latLngParser.SetLatLng();
            }
        }

        private void UpdateListing()
        {
            if (Listing != null)
            {
                ES_Listings.InsertUpdate(Page.Website, Listing);
            }
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }

            if (Page.HtmlDocument == null)
            {
                return;
            }
            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).InsertLinksAlternate();
                return;
            }
            if (Page.CanFollowLinks())
            {
                new HyperlinksIndexer(Page).Insert();
            }
        }

        private void RedirectIfError()
        {
            if (!Page.PageType.Equals(PageType.DownloadError) || Page.HttpStatusCode == null)
            {
                return;
            }

            int code = (int)Page.HttpStatusCode;
            if (code >= 300 && code < 400 && Config.INDEXER_ENABLED)
            {
                var redirectUrl = HttpClientDownloader.GetRedirectUrl();
                if (redirectUrl != null)
                {
                    new Indexer(Page).InsertUrl(redirectUrl);
                }
            }
        }
    }
}
