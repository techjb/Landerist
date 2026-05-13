using HtmlAgilityPack;
using landerist_library.Parse.Location.Candidates;

namespace landerist_library.Parse.Location.Extractors
{
    internal sealed class GoogleMapsIframeLocationExtractor
    {
        private readonly LocationCandidateFactory CandidateFactory;

        public GoogleMapsIframeLocationExtractor(LocationCandidateFactory candidateFactory)
        {
            CandidateFactory = candidateFactory;
        }

        public bool TryExtract(HtmlDocument htmlDocument, out LocationCandidate? candidate)
        {
            candidate = null;
            var iframes = htmlDocument.DocumentNode.Descendants("iframe");
            if (iframes == null)
            {
                return false;
            }
            foreach (var iframe in iframes)
            {
                var src = iframe.GetAttributeValue("src", string.Empty);
                if (string.IsNullOrEmpty(src))
                {
                    continue;
                }
                if (TryExtract(src, out candidate))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryExtract(string src, out LocationCandidate? candidate)
        {
            candidate = null;
            if (!src.Contains("https://www.google.com/maps/embed?pb="))
            {
                return false;
            }

            // in this order.
            if (src.Contains("!2d") && src.Contains("!3d"))
            {
                return TryExtract(src, "!2d", "!3d", false, out candidate);
            }
            if (src.Contains("!1d") && src.Contains("!2d"))
            {
                return TryExtract(src, "!1d", "!2d", true, out candidate);
            }
            return false;
        }

        private bool TryExtract(
            string src,
            string key1,
            string key2,
            bool inverted,
            out LocationCandidate? candidate)
        {
            candidate = null;
            try
            {
                var value1 = src[(src.IndexOf(key1, StringComparison.Ordinal) + key1.Length)..];
                value1 = value1[..value1.IndexOf('!')];

                var value2 = src[(src.IndexOf(key2, StringComparison.Ordinal) + key2.Length)..];
                value2 = value2[..value2.IndexOf('!')];

                // Mapping fix:
                // - default case (!2d = lng, !3d = lat): AddLatLng(lat, lng)
                // - inverted case (legacy order): AddLatLng(value1, value2)
                if (inverted)
                {
                    return CandidateFactory.TryCreate(value1, value2, false, LocationCandidateSources.GoogleMapsIframe, out candidate);
                }

                return CandidateFactory.TryCreate(value2, value1, false, LocationCandidateSources.GoogleMapsIframe, out candidate);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LatLngParser AddframeGoogleMapsLatLng", src, exception);
            }

            return false;
        }
    }
}
