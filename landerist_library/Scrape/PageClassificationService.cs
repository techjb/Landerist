using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Index;
using landerist_library.Pages;
using landerist_library.Parse.Location;
using landerist_orels.ES;

namespace landerist_library.Scrape
{
    internal sealed class PageClassificationService(Page page)
    {
        public bool TryApplyPreClassificationBeforeDownload()
        {
            if (page.Website.HtmlIndexingEnabled && Config.INDEXER_ENABLED)
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
            Pages.Pages.SetNextScrapeFromNow(page);
            return global::landerist_library.Pages.Pages.Update(page);
        }

        public bool ApplyClassificationResultAfterDownload(PageType? newPageType, Listing? newListing, bool waitingAIRequest)
        {
            if (waitingAIRequest)
            {
                if (!page.SetResponseBodyZipped())
                {
                    Logs.Log.WriteError("PageScraper SetPageType", "Failed to set response body zipped");
                    return false;
                }

                page.SetWaitingStatusAIRequest();
            }

            UpdatePageTypeAndListing(newPageType, newListing);
            page.SetLastScrape();
            Pages.Pages.SetNextScrape(page);
            return global::landerist_library.Pages.Pages.Update(page);
        }

        public bool ApplyParsedClassificationAfterParsing(PageType newPageType, Listing? listing)
        {
            if (newPageType.Equals(PageType.MayBeListing))
            {
                return false;
            }

            page.RemoveWaitingStatus();
            page.SetResponseBodyFromZipped();
            UpdatePageTypeAndListing(newPageType, listing);
            page.RemoveResponseBodyZipped();
            return global::landerist_library.Pages.Pages.Update(page);
        }

        private void UpdatePageTypeAndListing(PageType? newPageType, Listing? newListing)
        {
            page.SetPageType(newPageType);

            if (page.IsListing())
            {
                PublishListing(newListing);
                return;
            }

            if (page.IsNotListingByParser())
            {
                global::landerist_library.Pages.Pages.InsertToNotListingCache(page);
            }

            if (global::landerist_library.Pages.Pages.IsNotCanonicalListing(page) || global::landerist_library.Pages.Pages.IsRedirectToAnotherUrlListing(page))
            {
                HandleMovedListing(newListing);
            }

            var unpublishDecision = global::landerist_library.Pages.Pages.GetListingUnpublishDecision(page);
            if (unpublishDecision.ShouldUnpublish)
            {
                UnpublishListing(newListing, unpublishDecision);
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

            new Indexer(page).Insert(destinationUri);

            using var destinationPage = new Page(page.Website, destinationUri);
            if (!global::landerist_library.Pages.Pages.IsListingStatusPublished(destinationPage))
            {
                return;
            }

            UnpublishListing(newListing, CreateMovedListingUnpublishDecision());
        }

        private ListingUnpublishDecision CreateMovedListingUnpublishDecision()
        {
            return new ListingUnpublishDecision(
                true,
                ListingUnpublishDecisionReason.MovedListingDestinationPublished,
                page.PageType,
                page.HttpStatusCode,
                page.PageTypeCounter ?? 0,
                null);
        }

        private Uri? GetListingDestinationUri()
        {
            if (page.IsRedirectToAnotherUrl())
            {
                return new Indexer(page).GetUri(page.RedirectUrl);
            }

            if (page.IsNotCanonical())
            {
                return page.GetCanonicalUri();
            }

            return null;
        }

        private void PublishListing(Listing? newListing)
        {
            newListing ??= global::landerist_library.Pages.Pages.GetListing(page, true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandlePublishedListing", "NewListing is null");
                return;
            }

            newListing.SetPublished();
            new LocationParser(page, newListing).SetLocation();
            new LauIdParser(page.Website.CountryCode, newListing).SetLauIdAndLauName();
            ES_Listings.InsertUpdate(page.Website, newListing);
        }

        private void UnpublishListing(Listing? newListing, ListingUnpublishDecision? unpublishDecision = null)
        {
            newListing ??= global::landerist_library.Pages.Pages.GetListing(page, true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
                return;
            }

            newListing.SetUnpublished();
            ES_Listings.InsertUpdate(page.Website, newListing, unpublishDecision);
        }
    }
}

