using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Index;
using landerist_library.Parse.Location;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class PageScraper(Page page)
    {
        private readonly Page Page = page;

        private Listing? NewListing;

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
                //Console.WriteLine("PageScraper Download failed for: " + Page.Uri);
                return false;
            }
            (var newPageType, var newListing, var waitingAIRequest) = new PageTypeParser(Page).GetPageType();
            bool sucess = SetPageType(newPageType, newListing, waitingAIRequest);
            IndexPages();
            return sucess;
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
                    Logs.Log.WriteError("PageScraper SetPageType", "Failed to set response body zipped");
                    return false;
                }
                Page.SetWaitingStatusAIRequest();
            }

            SetPageType(newPageType, newListing);
            var setNextUpdate = !waitingAIRequest;
            return Page.Update(setNextUpdate);
        }      

        public void SetPageType(PageType? newPageType, Listing? newListing)
        {
            NewListing = newListing;
            Page.SetPageType(newPageType);

            if (Page.IsListing())
            {
                HandleListingPublished();
            }
            else if (Page.HaveToUnpublishListing())
            {
                HandleListingUnpublished();
            }
        }

        private void HandleListingPublished()
        {
            Page.SetListingStatusPublished();
            NewListing ??= Page.GetListing(true, true);

            if (NewListing == null) return;

            NewListing.SetPublished();
            SetLocation();
            SetLauId();
            UpdateListing();
        }

        private void HandleListingUnpublished()
        {
            Page.SetListingStatusUnpublished();
            NewListing ??= Page.GetListing(true, true);

            if (NewListing == null) return;

            NewListing.SetUnpublished();
            UpdateListing();
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

        private void UpdateListing()
        {
            if (NewListing == null)
            {
                return;
            }
            ES_Listings.InsertUpdate(Page.Website, NewListing);
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
