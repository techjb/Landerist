using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using landerist_orels.ES;

namespace landerist_library.Pages
{
    public partial class Page
    {
        public void SetListingParserInput()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument == null)
            {
                ListingParserInputNotChanged = false;
                ListingParserInputNotChangedCounter = null;
                return;
            }

            ListingParserInput = GetListingParserInput();
            if (string.IsNullOrEmpty(ListingParserInput))
            {
                ListingParserInputNotChanged = false;
                ListingParserInputNotChangedCounter = null;
                return;
            }

            string hash = Strings.GetHash(ListingParserInput);
            ListingParserInputNotChanged = hash == ListingParserInputHash;
            ListingParserInputNotChangedCounter = ListingParserInputNotChanged
                ? (short)Math.Min((ListingParserInputNotChangedCounter ?? 0) + 1, Config.MAX_PAGETYPE_COUNTER)
                : (short)0;
            ListingParserInputHash = hash;
        }

        public bool ListingParserInputHasNotChanged()
        {
            return ListingParserInputNotChanged && (IsListing() || IsNotListingByParser() || IsNotListingByCache());
        }

        public bool ListingParserInputIsError()
        {
            if (ListingParserInput == null)
            {
                return false;
            }
            return
                ListingParserInput.StartsWith("Not found", StringComparison.OrdinalIgnoreCase) ||
                ListingParserInput.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                ListingParserInput.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                ListingParserInput.Contains("algo salió mal", StringComparison.OrdinalIgnoreCase) ||
                ListingParserInput.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }

        public bool ListingParserInputIsTooLarge()
        {
            if (ListingParserInput is null)
            {
                return false;
            }
            return ListingParserInput.Length > Config.MAX_LISTINGPARSERINPUT_LENGTH;
        }

        public bool ListingParserInputIsTooShort()
        {
            if (string.IsNullOrEmpty(ListingParserInput))
            {
                return true;
            }
            return ListingParserInput.Length < Config.MIN_LISTINGPARSERINPUT_LENGTH;
        }

        public bool ListingParserInputIsAnotherListingInHost()
        {
            if (string.IsNullOrEmpty(ListingParserInput))
            {
                return false;
            }

            string query =
                "SELECT 1 " +
                "FROM " + Pages.PAGES + " " +
                "WHERE [HOST] = @Host AND " +
                "[UriHash] <> @UriHash AND " +
                "[ListingParserInputHash] = @ListingParserInputHash AND " +
                "[ListingStatus] IS NOT NULL";

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"Host", Host},
                {"UriHash", UriHash },
                {"ListingParserInputHash", ListingParserInputHash },
            });
        }

        public bool IsNotListingCache()
        {
            if (!Config.NOT_LISTING_CACHE_ENABLED || string.IsNullOrEmpty(ListingParserInputHash))
            {
                return false;
            }
            return NotListingsCache.IsNotListing(Host, ListingParserInputHash);
        }

        public bool InsertToNotListingCache()
        {
            return (ListingParserInputHash != null) && NotListingsCache.Insert(Host, ListingParserInputHash);
        }

        public Listing? GetListing(bool loadMedia, bool loadSources)
        {
            return ES_Listings.GetListing(this, loadMedia, loadSources);
        }

        public bool ContainsListing()
        {
            var listing = GetListing(false, false);
            return listing is not null;
        }

        public void SetListingStatusPublished()
        {
            ListingStatus = landerist_orels.ES.ListingStatus.published;
        }

        public void SetListingStatusUnpublished()
        {
            ListingStatus = landerist_orels.ES.ListingStatus.unpublished;
        }

        public bool IsListingStatusPublished()
        {
            return ListingStatus == landerist_orels.ES.ListingStatus.published;
        }

        public bool IsListingStatusUnPublished()
        {
            return ListingStatus == landerist_orels.ES.ListingStatus.unpublished;
        }

        public bool ContainsListingStatus()
        {
            return ListingStatus is not null;
        }

        public bool HaveToUnpublishListing()
        {
            return new ListingUnpublishEvaluator(this).ShouldUnpublish();
        }

        public bool IsNotCanonicalListing()
        {
            return IsNotCanonical() && ContainsListingStatus();
        }

        public bool IsRedirectToAnotherUrlListing()
        {
            return IsRedirectToAnotherUrl() && ContainsListingStatus();
        }
    }
}
