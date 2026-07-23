using landerist_library.Configuration;
using landerist_library.Application;
using landerist_library.Application.Persistence;
using landerist_library.Downloaders.Multiple;
using landerist_library.Index;
using landerist_library.Pages;
using landerist_library.Parse.PageTypeParser;
using landerist_library.Statistics;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    public class PageScraper
    {
        private readonly Page _page;
        private readonly PageClassificationService _classificationService;
        private readonly bool _useProxy;

        public PageScraper(Page page)
            : this(page, LanderistApplication.Services.PagePersistence)
        {
        }

        public PageScraper(
            Page page,
            IPagePersistenceService pagePersistence)
        {
            ArgumentNullException.ThrowIfNull(page);
            ArgumentNullException.ThrowIfNull(pagePersistence);
            _page = page;
            _classificationService = new PageClassificationService(page, pagePersistence);
            _useProxy = page.Website.UseProxy;
        }

        public PageScraper(Page page, bool useProxy)
            : this(page, useProxy, LanderistApplication.Services.PagePersistence)
        {
        }

        public PageScraper(
            Page page,
            bool useProxy,
            IPagePersistenceService pagePersistence)
            : this(page, pagePersistence)
        {
            _useProxy = useProxy;
        }

        public bool Scrape()
        {
            if (TryApplyNotModifiedBeforeDownload())
            {
                return true;
            }

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

        private bool TryApplyNotModifiedBeforeDownload()
        {
            if (!CanCheckConditionalHeaders() || Config.IsConfigurationLocal())
            {
                return false;
            }

            GlobalStatistics.InsertDailyCounter(StatisticsKey.PageConditionalHeadersCheck);

            var result = new ConditionalPageHeaderChecker(_useProxy).Check(_page);
            if (!result.NotModified)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(result.Etag))
            {
                _page.Etag = result.Etag;
            }

            if (!string.IsNullOrWhiteSpace(result.LastModified))
            {
                _page.LastModified = result.LastModified;
            }

            _page.RedirectUrl = result.RedirectUrl;

            GlobalStatistics.InsertDailyCounter(StatisticsKey.PageNotModified);
            HostStatistics.InsertDailyCounter(_page.Host, HostStatisticsKey.PageNotModified);
            return ApplyClassificationResultAfterDownload(_page.PageType, null, false);
        }

        private bool CanCheckConditionalHeaders()
        {
            return _page.PageType.HasValue &&
                (!string.IsNullOrWhiteSpace(_page.Etag) || !string.IsNullOrWhiteSpace(_page.LastModified));
        }

        public bool TryApplyPreClassificationBeforeDownload()
        {
            return _classificationService.TryApplyPreClassificationBeforeDownload();
        }

        public bool ApplyClassificationResultAfterDownload(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            return _classificationService.ApplyClassificationResultAfterDownload(newPageType, newListing, waitingAIRequest);
        }

        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing)
        {
            return _classificationService.ApplyParsedClassificationAfterParsing(newPageType, listing);
        }
    }
}
