using HtmlAgilityPack;
using landerist_library.Parse.Location.Candidates;
using landerist_library.Parse.Location.Validation;

namespace landerist_library.Parse.Location.Extractors
{
    internal sealed class HtmlLocationExtractor
    {
        private readonly MetaLocationExtractor MetaLocationExtractor;
        private readonly JsonLocationExtractor JsonLocationExtractor;
        private readonly GoogleMapsIframeLocationExtractor GoogleMapsIframeLocationExtractor;
        private readonly RegexLocationExtractor RegexLocationExtractor;

        public HtmlLocationExtractor(CountryCoordinateValidator coordinateValidator)
        {
            var candidateFactory = new LocationCandidateFactory(coordinateValidator);
            MetaLocationExtractor = new MetaLocationExtractor(candidateFactory);
            JsonLocationExtractor = new JsonLocationExtractor(candidateFactory);
            GoogleMapsIframeLocationExtractor = new GoogleMapsIframeLocationExtractor(candidateFactory);
            RegexLocationExtractor = new RegexLocationExtractor(candidateFactory);
        }

        public bool TryExtract(HtmlDocument htmlDocument, out LocationCandidate? candidate)
        {
            if (MetaLocationExtractor.TryExtract(htmlDocument, out candidate))
            {
                return true;
            }

            if (JsonLocationExtractor.TryExtract(htmlDocument, out candidate))
            {
                return true;
            }

            if (GoogleMapsIframeLocationExtractor.TryExtract(htmlDocument, out candidate))
            {
                return true;
            }

            return RegexLocationExtractor.TryExtract(htmlDocument, out candidate);
        }

        public bool TryExtractGoogleMapsIframe(string src, out LocationCandidate? candidate)
        {
            return GoogleMapsIframeLocationExtractor.TryExtract(src, out candidate);
        }

        public bool TryExtractRegex(
            string text,
            string regexPattern,
            string source,
            out LocationCandidate? candidate)
        {
            return RegexLocationExtractor.TryExtract(text, regexPattern, source, out candidate);
        }
    }
}
