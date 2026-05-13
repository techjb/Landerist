using HtmlAgilityPack;
using landerist_library.Parse.Location.Candidates;

namespace landerist_library.Parse.Location.Extractors
{
    internal sealed class MetaLocationExtractor
    {
        private readonly LocationCandidateFactory CandidateFactory;

        public MetaLocationExtractor(LocationCandidateFactory candidateFactory)
        {
            CandidateFactory = candidateFactory;
        }

        public bool TryExtract(HtmlDocument htmlDocument, out LocationCandidate? candidate)
        {
            candidate = null;
            var metaNodes = htmlDocument.DocumentNode.Descendants("meta");
            Dictionary<string, string> values = [];

            foreach (var metaNode in metaNodes)
            {
                var content = metaNode.GetAttributeValue("content", string.Empty).Trim();
                if (string.IsNullOrEmpty(content))
                {
                    continue;
                }

                foreach (var attributeName in new[] { "property", "name", "itemprop" })
                {
                    var key = metaNode.GetAttributeValue(attributeName, string.Empty).Trim();
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    values[key] = content;
                    if (CoordinateKeyMatcher.IsLatitudeKey(key) && TryFindLongitude(values, out var longitude))
                    {
                        return CandidateFactory.TryCreate(content, longitude, false, LocationCandidateSources.Html, out candidate);
                    }
                    if (CoordinateKeyMatcher.IsLongitudeKey(key) && TryFindLatitude(values, out var latitude))
                    {
                        return CandidateFactory.TryCreate(latitude, content, false, LocationCandidateSources.Html, out candidate);
                    }
                    if (CoordinateKeyMatcher.IsGeoPositionKey(key) && TrySplitGeoPosition(content, out var lat, out var lng))
                    {
                        return CandidateFactory.TryCreate(lat, lng, false, LocationCandidateSources.Html, out candidate);
                    }
                }
            }

            return false;
        }

        private static bool TryFindLatitude(Dictionary<string, string> values, out string latitude)
        {
            return TryFindCoordinate(values, CoordinateKeyMatcher.IsLatitudeKey, out latitude);
        }

        private static bool TryFindLongitude(Dictionary<string, string> values, out string longitude)
        {
            return TryFindCoordinate(values, CoordinateKeyMatcher.IsLongitudeKey, out longitude);
        }

        private static bool TryFindCoordinate(
            Dictionary<string, string> values,
            Func<string, bool> keyPredicate,
            out string coordinate)
        {
            coordinate = string.Empty;
            foreach (var pair in values)
            {
                if (keyPredicate(pair.Key))
                {
                    coordinate = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private static bool TrySplitGeoPosition(string content, out string latitude, out string longitude)
        {
            latitude = string.Empty;
            longitude = string.Empty;
            var parts = content.Split([';', ','], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return false;
            }

            latitude = parts[0];
            longitude = parts[1];
            return true;
        }
    }
}
