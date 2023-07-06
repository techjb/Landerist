using landerist_library.Websites;

namespace landerist_library.Parse.Listing
{
    public class IsListingParser
    {
        private static readonly HashSet<string> ProhibitedEndsSegments = new(StringComparer.OrdinalIgnoreCase)
        {
            "promociones",            
            "propiedades",
            "inmuebles",
            "mapa",
        };
        public static bool IsListing(Page page)
        {
            if (page == null)
            {
                return false;
            }
            if (page.IsMainPage())
            {
                return false;
            }
            if (IsProhibitedEndSegment(page.Uri))
            {
                return false;
            }

            page.SetResponseBodyText();
            if (ResponseBodyTextIsError(page.ResponseBodyText))
            {
                page.ResponseBodyText = null;
                return false;
            }            
            return true;
        }

        public static bool IsProhibitedEndSegment(Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.Query))
            {
                return false;
            }
            string[] segments = uri.Segments;
            string lastSegment = segments[^1].TrimEnd('/');
            return ProhibitedEndsSegments.Any(item => lastSegment.Equals(item, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ResponseBodyTextIsError(string? responseBodyText)
        {
            if (responseBodyText == null)
            {
                return false;
            }
            return
                responseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Página no encontrada", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
        }
    }
}
