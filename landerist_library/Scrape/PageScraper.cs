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

        private readonly PuppeteerDownloader Downloader = new();

        private readonly PageType? OldPageType = page.PageType;

        public bool Scrape()
        {
            Downloader.SetResponseBodyAndStatusCode(Page);
            SetPageType();
            UpdateListing();            
            IndexPages();

            return Page.Update();
        }
        private void SetPageType()
        {
            (var newPageType, NewListing) = PageTypeParser.GetPageType(Page);
            Page.SetPageType(newPageType);
        }

        private void UpdateListing()
        {
            if (Page.PageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(true);
                SetMedia();
                SetLocation();
                UpdateNewListing();
                return;
            }

            if (OldPageType != null && OldPageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(false);
                if (NewListing != null)
                {
                    NewListing.listingStatus = ListingStatus.unpublished;
                    NewListing.unlistingDate = DateTime.Now;
                    UpdateNewListing();
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

        private void UpdateNewListing()
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

            var redirectUrl = Downloader.GetRedirectUrl();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                new Indexer(Page).Insert(redirectUrl);
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
    }
}
