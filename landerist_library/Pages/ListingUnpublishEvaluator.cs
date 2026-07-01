namespace landerist_library.Pages
{
    public sealed record ListingUnpublishDecision(
        bool ShouldUnpublish,
        ListingUnpublishDecisionReason Reason,
        PageType? PageType,
        short? HttpStatusCode,
        int ActualEvidenceCount,
        int? RequiredEvidenceCount);

    public enum ListingUnpublishDecisionReason
    {
        ListingStatusIsNotPublished,
        PageTypeIsListing,
        PageTypeMayBeListing,
        NoUnpublishEvidence,
        EvidenceCounterBelowRequired,
        EvidenceCounterReachedRequired,
        MovedListingDestinationPublished
    }

    public class ListingUnpublishEvaluator
    {
        private const int RequiredGoneEvidenceCount = 1;
        private const int RequiredStrongEvidenceCount = 2;
        private const int RequiredDefaultEvidenceCount = 3;
        private readonly Page _page;

        public ListingUnpublishEvaluator(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            _page = page;
        }

        public bool ShouldUnpublish()
        {
            return Evaluate().ShouldUnpublish;
        }

        public ListingUnpublishDecision Evaluate()
        {
            if (!_page.IsListingStatusPublished())
            {
                return CreateDecision(false, ListingUnpublishDecisionReason.ListingStatusIsNotPublished, null);
            }

            if (_page.IsListing())
            {
                return CreateDecision(false, ListingUnpublishDecisionReason.PageTypeIsListing, null);
            }

            if (_page.IsMayBeListing())
            {
                return CreateDecision(false, ListingUnpublishDecisionReason.PageTypeMayBeListing, null);
            }

            var requiredEvidenceCount = GetRequiredEvidenceCount();
            if (requiredEvidenceCount is null)
            {
                return CreateDecision(false, ListingUnpublishDecisionReason.NoUnpublishEvidence, null);
            }

            if (GetActualEvidenceCount() < requiredEvidenceCount)
            {
                return CreateDecision(false, ListingUnpublishDecisionReason.EvidenceCounterBelowRequired, requiredEvidenceCount);
            }

            return CreateDecision(true, ListingUnpublishDecisionReason.EvidenceCounterReachedRequired, requiredEvidenceCount);
        }

        private ListingUnpublishDecision CreateDecision(
            bool shouldUnpublish,
            ListingUnpublishDecisionReason reason,
            int? requiredEvidenceCount)
        {
            return new ListingUnpublishDecision(
                shouldUnpublish,
                reason,
                _page.PageType,
                _page.HttpStatusCode,
                GetActualEvidenceCount(),
                requiredEvidenceCount);
        }

        private int GetActualEvidenceCount()
        {
            return _page.PageTypeCounter ?? 0;
        }

        private int? GetRequiredEvidenceCount()
        {
            return _page.PageType switch
            {
                PageType.HttpStatusCodeGone => RequiredGoneEvidenceCount,
                PageType.HttpStatusCodeNotFound => RequiredStrongEvidenceCount,
                PageType.NotListingByWebsiteRule => RequiredStrongEvidenceCount,
                PageType.DiscardedByListingUrlRegex => RequiredDefaultEvidenceCount,
                PageType.NotListingByParser => RequiredDefaultEvidenceCount,
                PageType.NotListingByCache => RequiredDefaultEvidenceCount,
                _ => null
            };
        }
    }
}