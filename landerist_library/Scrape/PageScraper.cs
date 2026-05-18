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
            if (!DownloadersPool.Download(_page, _useProxy))
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

        public bool TryApplyPreClassificationBeforeDownload()
        {
            if (_page.Website.HtmlIndexingEnabled && Config.INDEXER_ENABLED)
            {
                return false;
            }

            PageType? pageType = null;

            if (_page.IsMainPage())
            {
                pageType = PageType.MainPage;
            }
            else if (_page.Website.IsDiscardedByListingUrlRegex(_page.Uri))
            {
                pageType = PageType.DiscardedByListingUrlRegex;
            }

            if (pageType is null)
            {
                return false;
            }

            UpdatePageTypeAndListing(pageType, null);
            _page.SetNextScrapeFromNow();
            return _page.Update();
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
            _page.SetLastScrape();
            _page.SetNextScrape();
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
                _page.InsertToNotListingCache();
            }

            if (_page.IsNotCanonicalListing() || _page.IsRedirectToAnotherUrlListing())
            {
                HandleMovedListing(newListing);
            }

            if (_page.HaveToUnpublishListing())
            {
                UnpublishListing(newListing);
            }
        }

        private void HandleMovedListing(Listing? newListing)
        {
            var destinationUri = GetListingDestinationUri();
            if (destinationUri is null)
            {
                Logs.Log.WriteError("PageScraper HandleMovedListing", "Destination uri is null");
                return;
            }

            new Indexer(_page).Insert(destinationUri);

            using var destinationPage = new Page(_page.Website, destinationUri);
            if (!destinationPage.IsListingStatusPublished())
            {
                return;
            }

            UnpublishListing(newListing);
        }

        private Uri? GetListingDestinationUri()
        {
            if (_page.IsRedirectToAnotherUrl())
            {
                return new Indexer(_page).GetUri(_page.RedirectUrl);
            }

            if (_page.IsNotCanonical())
            {
                return _page.GetCanonicalUri();
            }

            return null;
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
            new LocationParser(_page, newListing).SetLocation();
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
            ES_Listings.InsertUpdate(_page.Website, newListing);
            _page.SetListingStatusUnpublished();
        }
    }
}
