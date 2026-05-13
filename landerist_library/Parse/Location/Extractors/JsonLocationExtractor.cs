using HtmlAgilityPack;
using landerist_library.Parse.Location.Candidates;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace landerist_library.Parse.Location.Extractors
{
    internal sealed class JsonLocationExtractor
    {
        private readonly LocationCandidateFactory CandidateFactory;

        public JsonLocationExtractor(LocationCandidateFactory candidateFactory)
        {
            CandidateFactory = candidateFactory;
        }

        public bool TryExtract(HtmlDocument htmlDocument, out LocationCandidate? candidate)
        {
            candidate = null;
            var scriptNodes = htmlDocument.DocumentNode.Descendants("script");

            foreach (var scriptNode in scriptNodes)
            {
                var type = scriptNode.GetAttributeValue("type", string.Empty);
                if (!IsJsonScriptType(type))
                {
                    continue;
                }

                var json = HtmlEntity.DeEntitize(scriptNode.InnerText).Trim();
                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }

                if (TryExtract(json, out candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryExtract(string json, out LocationCandidate? candidate)
        {
            candidate = null;
            try
            {
                var token = JToken.Parse(json);
                return TryExtract(token, out candidate);
            }
            catch
            {
                return false;
            }
        }

        private bool TryExtract(JToken token, out LocationCandidate? candidate)
        {
            candidate = null;
            if (token is JObject obj)
            {
                if (TryExtractGeoJsonCoordinates(obj, out candidate) ||
                    TryExtractObjectLatLng(obj, out candidate))
                {
                    return true;
                }
            }

            foreach (var child in token.Children())
            {
                if (TryExtract(child, out candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryExtractGeoJsonCoordinates(JObject obj, out LocationCandidate? candidate)
        {
            candidate = null;
            if (obj.TryGetValue("coordinates", StringComparison.OrdinalIgnoreCase, out var coordinates) &&
                coordinates is JArray { Count: >= 2 } array &&
                TryGetDouble(array[0], out var longitude) &&
                TryGetDouble(array[1], out var latitude))
            {
                return CandidateFactory.TryCreate(latitude, longitude, false, LocationCandidateSources.Html, out candidate);
            }

            return false;
        }

        private bool TryExtractObjectLatLng(JObject obj, out LocationCandidate? candidate)
        {
            candidate = null;
            double? latitude = null;
            double? longitude = null;

            foreach (var property in obj.Properties())
            {
                if (latitude == null &&
                    CoordinateKeyMatcher.IsLatitudeKey(property.Name) &&
                    TryGetDouble(property.Value, out var lat))
                {
                    latitude = lat;
                }
                else if (longitude == null &&
                    CoordinateKeyMatcher.IsLongitudeKey(property.Name) &&
                    TryGetDouble(property.Value, out var lng))
                {
                    longitude = lng;
                }
            }

            return latitude.HasValue &&
                longitude.HasValue &&
                CandidateFactory.TryCreate(latitude.Value, longitude.Value, false, LocationCandidateSources.Html, out candidate);
        }

        private static bool IsJsonScriptType(string type)
        {
            return string.IsNullOrWhiteSpace(type) ||
                type.Contains("json", StringComparison.OrdinalIgnoreCase) ||
                type.Equals("application/ld+json", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryGetDouble(JToken token, out double value)
        {
            if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
            {
                value = token.Value<double>();
                return true;
            }

            return double.TryParse(
                token.ToString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }
    }
}
