using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
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

        private readonly PageType? OldPageType = page.PageType;

        private SingleDownloader? SingleDownloader;

        private readonly Scraper? Scraper;

        private readonly bool UseProxy = false;


        public PageScraper(Page page, Scraper scraper, bool useProxy) : this(page)
        {
            Scraper = scraper;
            UseProxy = useProxy;
        }

        public bool Scrape()
        {
            if (!Download())
            {
                return false;
            }

            (var newPageType, var newListing, var waitingAIRequest) = PageTypeParser.GetPageType(Page);
            SetPageType(newPageType, newListing, waitingAIRequest);
            IndexPages();
            return true;
        }


        private bool Download()
        {
            if (Config.MULTIPLE_DOWNLOADERS_ENABLED)
            {
                return DownloadMultipleDownloaders();
            }
            return DownloadSingleDownloader();
        }

        private bool DownloadMultipleDownloaders()
        {
            if (Scraper is null)
            {
                return false;
            }
            SingleDownloader = MultipleDownloader.GetDownloader(UseProxy);
            if (SingleDownloader is null)
            {
                return false;
            }
            return SingleDownloader.Download(Page);
        }

        private bool DownloadSingleDownloader()
        {
            SingleDownloader = new(UseProxy);
            if (!SingleDownloader.IsAvailable(UseProxy))
            {
                Logs.Log.WriteInfo("PageScraper DownloadPageSingleDownloader", "Downloader not available");
                return false;
            }
            SingleDownloader.Download(Page);
            SingleDownloader.CloseBrowser();
            return true;
        }

        public bool SetPageType(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            if (waitingAIRequest)
            {
                if (!Page.SetResponseBodyZipped())
                {
                    return false;
                }
                Page.SetWaitingStatusAIRequest();
                return Page.Update(false);
            }
            SetPageType(newPageType, newListing);
            return Page.Update(true);
        }

        public void SetPageType(PageType? newPageType, Listing? newListing)
        {
            NewListing = newListing;
            Page.SetPageType(newPageType);
            UpdateListing();
        }


        private void UpdateListing()
        {
            if (Page.PageType.Equals(PageType.Listing))
            {
                NewListing ??= Page.GetListing(true);
                //SetMedia();
                SetLocation();
                SetLauId();
                UpdateNewListing();
                return;
            }

            if (!Page.PageType.Equals(PageType.MayBeListing) &&
                OldPageType != null &&
                OldPageType.Equals(PageType.Listing))
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
            if (NewListing != null && Config.MEDIA_PARSER_ENABLED)
            {
                MediaParser mediaParser = new(Page);
                mediaParser.AddMedia(NewListing);
            }
        }

        private void SetLocation()
        {
            if (NewListing != null)
            {
                new LatLngParser(Page, NewListing).SetLatLng();
            }
        }

        private void SetLauId()
        {
            if (NewListing != null)
            {
                new LauIdParser(Page, NewListing).SetLauId();
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

            if (SingleDownloader == null)
            {
                return;
            }
            var redirectUrl = SingleDownloader.GetRedirectUrl();
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
            new HyperlinksIndexer(Page).Insert();
        }

        public Listing? GetListing()
        {
            return NewListing;
        }
    }
}
