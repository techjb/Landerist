using landerist_library.Application.Listings;
using landerist_library.Application.Logging;
using landerist_library.Application.Persistence;
using landerist_library.Application.Scraping;
using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping
{
    public class PageScraper
    {
        private readonly Page _page;
        private readonly PageClassificationService _classificationService;
        private readonly PageScrapePipelineServices _pipeline;
        private readonly bool _useProxy;

        public PageScraper(
            Page page,
            IPagePersistenceService pagePersistence,
            IApplicationLogger logger,
            IListingLifecycleService listingLifecycle,
            PageScrapePipelineServices pipeline)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(listingLifecycle);
            ArgumentNullException.ThrowIfNull(pipeline);

            _page = page;
            _classificationService = new PageClassificationService(
                page,
                pagePersistence,
                logger,
                listingLifecycle,
                pipeline.Scheduling,
                pipeline.IndexerEnabled);
            _pipeline = pipeline;
            _useProxy = page.Website.UseProxy;
        }

        public PageScraper(
            Page page,
            bool useProxy,
            IPagePersistenceService pagePersistence,
            IApplicationLogger logger,
            IListingLifecycleService listingLifecycle,
            PageScrapePipelineServices pipeline)
            : this(page, pagePersistence, logger, listingLifecycle, pipeline)
        {
            _useProxy = useProxy;
        }

        public bool Scrape()
        {
            PageAcquisitionStatus status = _pipeline.Acquisition.Acquire(_page, _useProxy);
            return ProcessAcquisitionResult(status);
        }

        public async Task<bool> ScrapeAsync(CancellationToken cancellationToken = default)
        {
            PageAcquisitionStatus status = await _pipeline.Acquisition
                .AcquireAsync(_page, _useProxy, cancellationToken)
                .ConfigureAwait(false);
            return await ProcessAcquisitionResultAsync(status, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task<bool> ProcessAcquisitionResultAsync(
            PageAcquisitionStatus acquisitionStatus,
            CancellationToken cancellationToken)
        {
            if (acquisitionStatus == PageAcquisitionStatus.DownloadFailed)
            {
                return false;
            }

            if (acquisitionStatus == PageAcquisitionStatus.NotModified)
            {
                return await ApplyClassificationResultAfterDownloadAsync(
                    _page.PageType,
                    null,
                    false,
                    cancellationToken).ConfigureAwait(false);
            }

            PageClassificationResult classification =
                _pipeline.Classifier.Classify(_page);
            bool success = await ApplyClassificationResultAfterDownloadAsync(
                classification.PageType,
                classification.Listing,
                classification.WaitingAiRequest,
                cancellationToken).ConfigureAwait(false);

            if (success)
            {
                _pipeline.Indexing.Index(_page);
            }

            return success;
        }

        private bool ProcessAcquisitionResult(PageAcquisitionStatus acquisitionStatus)
        {
            if (acquisitionStatus == PageAcquisitionStatus.DownloadFailed)
            {
                return false;
            }

            if (acquisitionStatus == PageAcquisitionStatus.NotModified)
            {
                return ApplyClassificationResultAfterDownload(_page.PageType, null, false);
            }

            var classification = _pipeline.Classifier.Classify(_page);
            var success = ApplyClassificationResultAfterDownload(
                classification.PageType,
                classification.Listing,
                classification.WaitingAiRequest);

            if (success)
            {
                _pipeline.Indexing.Index(_page);
            }

            return success;
        }

        public Task<bool> TryApplyPreClassificationBeforeDownloadAsync(
            CancellationToken cancellationToken = default) =>
            _classificationService.TryApplyPreClassificationBeforeDownloadAsync(
                cancellationToken);
        public bool TryApplyPreClassificationBeforeDownload()
        {
            return _classificationService.TryApplyPreClassificationBeforeDownload();
        }

        public bool ApplyClassificationResultAfterDownload(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            return _classificationService.ApplyClassificationResultAfterDownload(newPageType, newListing, waitingAIRequest);
        }

        public Task<bool> ApplyClassificationResultAfterDownloadAsync(
            PageType? newPageType,
            Listing? newListing,
            bool waitingAIRequest,
            CancellationToken cancellationToken = default) =>
            _classificationService.ApplyClassificationResultAfterDownloadAsync(
                newPageType,
                newListing,
                waitingAIRequest,
                cancellationToken);
        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing)
        {
            return _classificationService.ApplyParsedClassificationAfterParsing(newPageType, listing);
        }
    }
}

