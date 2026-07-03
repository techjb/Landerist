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
            page.SetNextScrapeFromNow();
            return page.Update();
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
            page.SetNextScrape();
            return page.Update();
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
            return page.Update();
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
                page.InsertToNotListingCache();
            }

            if (page.IsNotCanonicalListing() || page.IsRedirectToAnotherUrlListing())
            {
                HandleMovedListing(newListing);
            }

            var unpublishDecision = page.GetListingUnpublishDecision();
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
            if (!destinationPage.IsListingStatusPublished())
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
            newListing ??= page.GetListing(true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandlePublishedListing", "NewListing is null");
                return;
            }

            newListing.SetPublished();
            new LocationParser(page, newListing).SetLocation();
            new LauIdParser(page.Website.CountryCode, newListing).SetLauIdAndLauName();
            ES_Listings.InsertUpdate(page.Website, newListing);
            page.SetListingStatusPublished();
        }

        private void UnpublishListing(Listing? newListing, ListingUnpublishDecision? unpublishDecision = null)
        {
            newListing ??= page.GetListing(true, true);
            if (newListing == null)
            {
                Logs.Log.WriteError("PageScraper HandleUnpublishedListing", "NewListing is null");
                return;
            }

            newListing.SetUnpublished();
            ES_Listings.InsertUpdate(page.Website, newListing, unpublishDecision);
            page.SetListingStatusUnpublished();
        }
    }
}

