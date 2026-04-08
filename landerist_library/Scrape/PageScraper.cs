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
    public class PageScraper
    {
        private readonly Page _page;
        private readonly Scraper? _scraper;
        private readonly bool _useProxy;

        private Listing? _newListing;
        private SingleDownloader? _singleDownloader;

        public PageScraper(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            _page = page;
        }

        public PageScraper(Page page, Scraper scraper, bool useProxy) : this(page)
        {
            _scraper = scraper;
            _useProxy = useProxy;
        }

        public bool Scrape()
        {
            if (!Download())
            {
                //Console.WriteLine("PageScraper Download failed for: " + _page.Uri);
                return false;
            }

            (var newPageType, var newListing, var waitingAIRequest) = new PageTypeParser(_page).GetPageType();
            var success = SetPageType(newPageType, newListing, waitingAIRequest);

            if (success)
            {
                IndexPages();
            }

            return success;
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
            if (_scraper is null)
            {
                return false;
            }

            _singleDownloader = MultipleDownloader.GetDownloader(_useProxy);
            if (_singleDownloader is null)
            {
                return false;
            }

            return _singleDownloader.Download(_page);
        }

        private bool DownloadSingleDownloader()
        {
            _singleDownloader = new SingleDownloader(_useProxy);
            if (!_singleDownloader.IsAvailable(_useProxy))
            {
                Logs.Log.WriteInfo("PageScraper DownloadPageSingleDownloader", "Downloader not available");
                return false;
            }

            try
            {
                return _singleDownloader.Download(_page);
            }
            finally
            {
                _singleDownloader.CloseBrowser();
            }
        }

        public bool SetPageType(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            if (waitingAIRequest)
            {
                if (!_page.SetResponseBodyZipped())
                {
                    Logs.Log.WriteError("PageScraper SetPageType", "Failed to set response body zipped");
                    return false;
                }

                _page.SetWaitingStatusAIRequest();
            }

            SetPageType(newPageType, newListing);
            var setNextUpdate = !waitingAIRequest;
            return _page.Update(setNextUpdate);
        }

        public bool SetPageTypeAfterParsing(PageType newPageType, Listing? listing)
        {
            if (newPageType.Equals(PageType.MayBeListing))
            {
                return false;
            }

            _page.RemoveWaitingStatus();
            _page.SetResponseBodyFromZipped();
            SetPageType(newPageType, listing);
            //new TrainingData().Insert(Page);
            _page.RemoveResponseBodyZipped();
            return _page.Update(true);
        }

        public void SetPageType(PageType? newPageType, Listing? newListing)
        {
            _newListing = newListing;
            _page.SetPageType(newPageType);

            if (_page.IsListing())
            {
                HandlePublishedListing();
                return;
            }

            if (_page.IsNotListingByParser())
            {
                _page.InsertNotListingResponseBodyText();
            }

            if (_page.HaveToUnpublishListing())
            {
                HandleUnpublishedListing();
            }

            if (_page.IsNotCanonicalListing() || _page.IsRedirectToAnotherUrlListing())
            {
                // todo: handle not canonical listing
            }
        }

        private void HandlePublishedListing()
        {
            _newListing ??= _page.GetListing(true, true);
            if (_newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandlePublishedListing", "NewListing is null");
                return;
            }

            _newListing.SetPublished();
            new LocationParser(_page, _newListing).SetLatLng();
            new LauIdParser(_page.Website.CountryCode, _newListing).SetLauIdAndLauName();
            ES_Listings.InsertUpdate(_page.Website, _newListing);
            _page.SetListingStatusPublished();
        }

        private void HandleUnpublishedListing()
        {
            _newListing ??= _page.GetListing(true, true);
            if (_newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
                return;
            }

            _newListing.SetUnpublished();
            ES_Listings.InsertUpdate(_page.Website, _newListing);
            _page.SetListingStatusUnpublished();
        }

        private void IndexPages()
        {
            if (!Config.INDEXER_ENABLED)
            {
                return;
            }

            if (_page.Website.AchievedMaxNumberOfPages())
            {
                return;
            }

            if (!string.IsNullOrEmpty(_page.RedirectUrl))
            {
                new Indexer(_page).Insert(_page.RedirectUrl);
                return;
            }

            if (_page.ContainsMetaRobotsNoFollow())
            {
                return;
            }

            if (_page.PageType.Equals(PageType.IncorrectLanguage))
            {
                new LinkAlternateIndexer(_page).Insert();
                return;
            }

            if (_page.PageType.Equals(PageType.NotCanonical))
            {
                new CanonicalIndexer(_page).Insert();
                return;
            }

            new HyperlinksIndexer(_page).Insert();
        }

        public Listing? GetListing()
        {
            return _newListing;
        }
    }
}
