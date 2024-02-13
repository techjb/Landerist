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

        private Listing? NewListing;

        private readonly HttpClientDownloader HttpClientDownloader = new();

        public bool Scrape()
        {
            string? responseBody = HttpClientDownloader.Get(Page);
            Page.SetResponseBody(responseBody);

            SetPageType();
            UpdateIfListing();
            UpdateIfUnPublishedListing();
            RedirectIfError();
            IndexPages();

            return Page.Update();
        }
        private void SetPageType()
        {
            var (newPageType, newListing) = PageTypeParser.GetPageType(Page);
            NewListing = newListing;

            newPageType = SetToUnpublished(newPageType);
            Page.SetPageType(newPageType);
        }

        private PageType? SetToUnpublished(PageType? newPageType)
        {
            if (Page.PageType != null && Page.PageType.Equals(PageType.Listing) && newPageType != PageType.Listing)
            {
                newPageType = PageType.UnpublishedListing;
            }
            return newPageType;
        }

        private void UpdateIfListing()
        {
            if (Page.PageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(true);
                SetMedia();
                SetLocation();
                UpdateListing();
            }
        }

        private void UpdateIfUnPublishedListing()
        {
            if (Page.PageType.Equals(PageType.UnpublishedListing))
            {
                NewListing ??= Page.GetListing(true);
                if (NewListing != null)
                {
                    NewListing.listingStatus = ListingStatus.unpublished;
                    NewListing.unlistingDate = DateTime.Now;
                    UpdateListing();
                }
            }
        }



        private void SetMedia()
        {
            if (NewListing != null)
            {
                MediaParser mediaParser = new(Page);
                mediaParser.AddMedia(NewListing);
            }
        }

        private void SetLocation()
        {
            if (NewListing != null)
            {
                LatLngParser latLngParser = new(Page, NewListing);
                latLngParser.SetLatLng();
            }
        }

        private void UpdateListing()
        {
            if (NewListing != null)
            {
                ES_Listings.InsertUpdate(Page.Website, NewListing);
            }
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }
            if (Page.Website.AchievedMaxNumberOfPages())
            {
                return;
            }

            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).InsertLinksAlternate();
                return;
            }
            if (Page.NotIndexable())
            {
                return;
            }
            new HyperlinksIndexer(Page).Insert();
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
