using HtmlAgilityPack;
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
                ? (short)Math.Min((ListingParserInputNotChangedCounter ?? 0) + 1, Rules.MaxPageTypeCounter)
                : (short)0;
            ListingParserInputHash = hash;
        }

        public bool ListingParserInputHasNotChanged()
        {
            return ListingParserInputNotChanged && (IsListing() || IsNotListingByParser() || IsNotListingByCache() || IsNotListingByWebsiteRule());
        }

        public bool MatchesWebsiteListingUnavailableRule()
        {
            if (Website.MatchesListingUnavailableRegex(ListingParserInput))
            {
                return true;
            }

            return Website.MatchesListingUnavailableRegex(GetListingParserInputText());
        }

        private string? GetListingParserInputText()
        {
            if (string.IsNullOrWhiteSpace(ListingParserInput))
            {
                return null;
            }

            try
            {
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(ListingParserInput);
                return HtmlEntity.DeEntitize(htmlDocument.DocumentNode.InnerText);
            }
            catch (Exception)
            {
                return null;
            }
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
            return ListingParserInput.Length > Rules.MaxListingParserInputLength;
        }

        public bool ListingParserInputIsTooShort()
        {
            if (string.IsNullOrEmpty(ListingParserInput))
            {
                return true;
            }
            return ListingParserInput.Length < Rules.MinListingParserInputLength;
        }

    }
}
