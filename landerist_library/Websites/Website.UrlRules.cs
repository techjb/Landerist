using System.Text.RegularExpressions;

namespace landerist_library.Websites
{
    public partial class Website
    {
        public bool IsDiscardedByIndexUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, IndexUrlRegex, "IndexUrlRegex");
        }

        public bool IsDiscardedByListingUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, ListingUrlRegex, "ListingUrlRegex");
        }

        public bool MatchesListingUrlRegex(Uri uri)
        {
            return MatchesRegex(uri, ListingUrlRegex, "ListingUrlRegex");
        }

        public bool IsDiscardedBySitemapUrlRegex(Uri uri)
        {
            return IsDiscardedByRegex(uri, SitemapUrlRegex, "SitemapUrlRegex");
        }       

        private bool IsDiscardedByRegex(Uri uri, string? regexPattern, string regexFieldName)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (string.IsNullOrWhiteSpace(regexPattern))
            {
                return false;
            }

            return !MatchesRegex(uri, regexPattern, regexFieldName);
        }

        private bool MatchesRegex(Uri uri, string? regexPattern, string regexFieldName)
        {
            ArgumentNullException.ThrowIfNull(uri);

            if (string.IsNullOrWhiteSpace(regexPattern))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(
                    uri.AbsoluteUri,
                    regexPattern,
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException exception)
            {
                Logs.Log.WriteError(
                    "Website IsDiscardedByRegex",
                    $"{Host} {regexFieldName} {regexPattern}",
                    exception);

                return false;
            }
        }
    }
}
