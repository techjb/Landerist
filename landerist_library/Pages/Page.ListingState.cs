using System.Security.Cryptography;
using System.Text;

namespace landerist_library.Pages;

public partial class Page
{
    public void ApplyListingParserInput(
        string? parserInput,
        bool contentAvailable = true)
    {
        if (!contentAvailable)
        {
            ListingParserInputNotChanged = false;
            ListingParserInputNotChangedCounter = null;
            return;
        }

        ListingParserInput = parserInput;
        if (string.IsNullOrEmpty(parserInput))
        {
            ListingParserInputNotChanged = false;
            ListingParserInputNotChangedCounter = null;
            return;
        }

        string hash = GetInputHash(parserInput);
        ListingParserInputNotChanged = hash == ListingParserInputHash;
        ListingParserInputNotChangedCounter = ListingParserInputNotChanged
            ? (short)Math.Min(
                (ListingParserInputNotChangedCounter ?? 0) + 1,
                Rules.MaxPageTypeCounter)
            : (short)0;
        ListingParserInputHash = hash;
    }

    public bool ListingParserInputHasNotChanged()
    {
        return ListingParserInputNotChanged &&
            (IsListing() ||
                IsNotListingByParser() ||
                IsNotListingByCache() ||
                IsNotListingByWebsiteRule());
    }

    public bool ListingParserInputIsError()
    {
        if (ListingParserInput is null)
        {
            return false;
        }

        return ListingParserInput.StartsWith(
                "Not found",
                StringComparison.OrdinalIgnoreCase) ||
            ListingParserInput.StartsWith(
                "Error",
                StringComparison.OrdinalIgnoreCase) ||
            ListingParserInput.StartsWith(
                "404",
                StringComparison.OrdinalIgnoreCase) ||
            ListingParserInput.Contains(
                "algo saliÃ³ mal",
                StringComparison.OrdinalIgnoreCase) ||
            ListingParserInput.Contains(
                "Page Not found",
                StringComparison.OrdinalIgnoreCase);
    }

    public bool ListingParserInputIsTooLarge() =>
        ListingParserInput is not null &&
        ListingParserInput.Length > Rules.MaxListingParserInputLength;

    public bool ListingParserInputIsTooShort() =>
        string.IsNullOrEmpty(ListingParserInput) ||
        ListingParserInput.Length < Rules.MinListingParserInputLength;

    private static string GetInputHash(string input)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }
}