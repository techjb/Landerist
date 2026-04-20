using landerist_library.Configuration;

namespace landerist_library.Pages
{
    public class ListingUnpublishEvaluator
    {
        private readonly Page _page;

        public ListingUnpublishEvaluator(Page page)
        {
            ArgumentNullException.ThrowIfNull(page);
            _page = page;
        }

        public bool ShouldUnpublish()
        {
            if (!_page.IsListingStatusPublished())
            {
                return false;
            }

            if (_page.IsListing() || _page.IsMayBeListing())
            {
                return false;
            }

            if (!HasUnpublishEvidence())
            {
                return false;
            }

            return (_page.PageTypeCounter ?? 0) >= GetRequiredCounter();
        }

        private bool HasUnpublishEvidence()
        {
            return IsStrongUnpublishEvidence() ||
                _page.IsNotListingByParser() ||
                _page.IsNotCanonical() ||
                _page.IsRedirectToAnotherUrl() ||
                _page.IsDiscardedByListingUrlRegex()
                ;
        }

        private bool IsStrongUnpublishEvidence()
        {
            return _page.IsHttpStatusCodeNotFound() || _page.IsHttpStatusCodeGone();
        }

        private int GetRequiredCounter()
        {
            if (IsStrongUnpublishEvidence())
            {
                return 2;
            }
            return 3;
        }
    }
}
