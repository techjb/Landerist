namespace landerist_library.Pages
{
    public class ListingUnpublishEvaluator
    {
        private const int GoneCounter = 1;
        private const int StrongEvidenceCounter = 2;
        private const int DefaultEvidenceCounter = 3;
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

            var requiredCounter = GetRequiredCounter();
            if (requiredCounter is null)
            {
                return false;
            }

            return (_page.PageTypeCounter ?? 0) >= requiredCounter;
        }

        private int? GetRequiredCounter()
        {
            return _page.PageType switch
            {
                PageType.HttpStatusCodeGone => GoneCounter,
                PageType.HttpStatusCodeNotFound => StrongEvidenceCounter,
                PageType.NotListingByWebsiteRule => StrongEvidenceCounter,
                PageType.DiscardedByListingUrlRegex => DefaultEvidenceCounter,
                PageType.NotListingByParser => DefaultEvidenceCounter,
                PageType.NotListingByCache => DefaultEvidenceCounter,
                _ => null
            };
        }
    }
}
