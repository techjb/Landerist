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

        public async Task<bool> TryApplyPreClassificationBeforeDownloadAsync(
            CancellationToken cancellationToken = default)
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

            await UpdatePageTypeAndListingAsync(
                pageType,
                null,
                cancellationToken).ConfigureAwait(false);
            _scheduling.SetNextScrapeFromNow(page);
            return await _pagePersistence
                .UpdateAsync(page, cancellationToken)
                .ConfigureAwait(false);
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

        public async Task<bool> ApplyClassificationResultAfterDownloadAsync(
            PageType? newPageType,
            Listing? newListing,
            bool waitingAIRequest,
            CancellationToken cancellationToken = default)
        {
            if (waitingAIRequest)
            {
                if (!page.SetResponseBodyZipped())
                {
                    _logger.WriteError(
                        "PageScraper SetPageType",
                        "Failed to set response body zipped");
                    return false;
                }

                page.SetWaitingStatusAIRequest();
            }

            await UpdatePageTypeAndListingAsync(
                newPageType,
                newListing,
                cancellationToken).ConfigureAwait(false);
            page.SetLastScrape();
            _scheduling.SetNextScrape(page);
            return await _pagePersistence
                .UpdateAsync(page, cancellationToken)
                .ConfigureAwait(false);
        }
        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing) =>
            _parsedClassification.Apply(page, newPageType, listing);

        private async Task UpdatePageTypeAndListingAsync(
            PageType? newPageType,
            Listing? newListing,
            CancellationToken cancellationToken)
        {
            page.SetPageType(newPageType);
            await _listingLifecycle
                .ApplyAsync(page, newListing, cancellationToken)
                .ConfigureAwait(false);
        }
        private void UpdatePageTypeAndListing(PageType? newPageType, Listing? newListing)
        {
            page.SetPageType(newPageType);
            _listingLifecycle.Apply(page, newListing);
        }
    }
}
