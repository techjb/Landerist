using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Downloaders.Multiple;
using landerist_library.Index;
using landerist_library.Pages;
using landerist_library.Parse.Location;
using landerist_library.Parse.PageTypeParser;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class PageScraper
    {
        private readonly Page _page;
        private readonly bool _useProxy;

        public PageScraper(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            _page = page;
        }

        public PageScraper(Page page, bool useProxy) : this(page)
        {
            _useProxy = useProxy;
        }

        public bool Scrape()
        {
            if (!Download())
            {
                return false;
            }

            (var newPageType, var newListing, var waitingAIRequest) = new PageTypeParser(_page).GetPageType();
            var success = ApplyClassificationResultAfterDownload(newPageType, newListing, waitingAIRequest);

            if (success)
            {
                new Indexer(_page).IndexPages();
            }

            return success;
        }

        private bool Download()
        {
            if (Config.DOWNLOADERS_POOL_ENABLED)
            {
                return DownloadersPool.Download(_page, _useProxy);
            }

            var singleDownloader = new SingleDownloader(_useProxy);
            if (!singleDownloader.TryReserve(_useProxy))
            {
                Logs.Log.WriteInfo("PageScraper DownloadPageSingleDownloader", "Downloader not available");
                return false;
            }

            try
            {
                return singleDownloader.Download(_page);
            }
            finally
            {
                singleDownloader.CloseBrowser();
            }
        }


        public bool ApplyClassificationResultAfterDownload(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
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

            UpdatePageTypeAndListing(newPageType, newListing);
            _page.SetNextUpdate();            
            return _page.Update();
        }

        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing)
        {
            if (newPageType.Equals(PageType.MayBeListing))
            {
                return false;
            }

            _page.RemoveWaitingStatus();
            _page.SetResponseBodyFromZipped();
            UpdatePageTypeAndListing(newPageType, listing);
            _page.RemoveResponseBodyZipped();
            return _page.Update();
        }

        private void UpdatePageTypeAndListing(PageType? newPageType, Listing? newListing)
        {
            _page.SetPageType(newPageType);

            if (_page.IsListing())
            {
                PublishListing(newListing);
                return;
            }

            if (_page.IsNotListingByParser())
            {
                _page.InsertNotListingResponseBodyText();
            }

            if (_page.HaveToUnpublishListing())
            {
                UnpublishListing(newListing);
            }

            if (_page.IsNotCanonicalListing() || _page.IsRedirectToAnotherUrlListing())
            {
                // todo: handle not canonical listing
            }
        }


        private void PublishListing(Listing? newListing)
        {
            newListing ??= _page.GetListing(true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandlePublishedListing", "NewListing is null");
                return;
            }

            newListing.SetPublished();
            new LocationParser(_page, newListing).SetLatLng();
            new LauIdParser(_page.Website.CountryCode, newListing).SetLauIdAndLauName();
            ES_Listings.InsertUpdate(_page.Website, newListing);
            _page.SetListingStatusPublished();
        }

        private void UnpublishListing(Listing? newListing)
        {
            newListing ??= _page.GetListing(true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
                return;
            }

            newListing.SetUnpublished();
            //_page.Website.DecreaseNumListings();
            ES_Listings.InsertUpdate(_page.Website, newListing);
            _page.SetListingStatusUnpublished();
        }
    }
}
