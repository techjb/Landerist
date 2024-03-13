using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders;
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

        //private readonly HttpClientDownloader Downloader = new();
        private readonly PuppeteerDownloader Downloader = new();

        public bool Scrape()
        {
            Downloader.SetResponseBodyAndStatusCode(Page);
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
            if (Page.ContainsMetaRobotsNoFollow())
            {
                return;
            }
            if (Page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(Page).Insert();
                return;
            }
            if (Page.PageType.Equals(PageType.NotCanonical))
            {
                new CanonicalIndexer(Page).Insert();
                return;
            }
            if (Page.ContainsMetaRobotsNoIndex())
            {
                return;
            }
            new HyperlinksIndexer(Page).Insert();
        }

        private void RedirectIfError()
        {
            if (Page.HttpStatusCode == null)
            {
                return;
            }

            int code = (int)Page.HttpStatusCode;
            if (code >= 300 && code < 400 && Config.INDEXER_ENABLED)
            {
                var redirectUrl = Downloader.GetRedirectUrl();
                if (redirectUrl != null)
                {
                    new Indexer(Page).Insert(redirectUrl);
                }
            }
        }
    }
}
