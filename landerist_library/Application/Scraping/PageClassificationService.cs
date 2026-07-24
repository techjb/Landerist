using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping
{
    internal sealed class PageClassificationService
    {
        private readonly Page page;
        private readonly IPagePersistenceService _pagePersistence;
        private readonly IApplicationLogger _logger;
        private readonly IListingLifecycleService _listingLifecycle;
        private readonly IPageSchedulingService _scheduling;
        private readonly IParsedPageClassificationService _parsedClassification;
        private readonly bool _indexerEnabled;

        public PageClassificationService(
            Page page,
            IPagePersistenceService pagePersistence,
            IApplicationLogger logger,
            IListingLifecycleService listingLifecycle,
            IPageSchedulingService scheduling,
            bool indexerEnabled)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(listingLifecycle);
            ArgumentNullException.ThrowIfNull(scheduling);

            this.page = page;
            _pagePersistence = pagePersistence;
            _logger = logger;
            _listingLifecycle = listingLifecycle;
            _scheduling = scheduling;
            _indexerEnabled = indexerEnabled;
            _parsedClassification = new ParsedPageClassificationService(
                pagePersistence,
                listingLifecycle);
        }

        public bool TryApplyPreClassificationBeforeDownload()
        {
            if (page.Website.HtmlIndexingEnabled && _indexerEnabled)
            {
                return false;
            }

            PageType? pageType = null;

            if (page.IsMainPage())
            {
                pageType = PageType.MainPage;
            }
            else if (page.Website.IsDiscardedByListingUrlRegex(page.Uri))
            {
                pageType = PageType.DiscardedByListingUrlRegex;
            }

            if (pageType is null)
            {
                return false;
            }

            UpdatePageTypeAndListing(pageType, null);
            _scheduling.SetNextScrapeFromNow(page);
            return _pagePersistence.Update(page);
        }

        public bool ApplyClassificationResultAfterDownload(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            if (waitingAIRequest)
            {
                if (!page.SetResponseBodyZipped())
                {
                    _logger.WriteError("PageScraper SetPageType", "Failed to set response body zipped");
                    return false;
                }

                page.SetWaitingStatusAIRequest();
            }

            UpdatePageTypeAndListing(newPageType, newListing);
            page.SetLastScrape();
            _scheduling.SetNextScrape(page);
            return _pagePersistence.Update(page);
        }

        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing) =>
            _parsedClassification.Apply(page, newPageType, listing);

        private void UpdatePageTypeAndListing(PageType? newPageType, Listing? newListing)
        {
            page.SetPageType(newPageType);
            _listingLifecycle.Apply(page, newListing);
        }
    }
}
