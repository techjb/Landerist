using HtmlAgilityPack;
using landerist_library.Parse.Location.Candidates;
using System.Globalization;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Location.Extractors
{
    internal sealed class RegexLocationExtractor
    {
        private const string NumberPattern = @"-?\d+(?:\.\d+)?";

        private readonly LocationCandidateFactory CandidateFactory;

        public RegexLocationExtractor(LocationCandidateFactory candidateFactory)
        {
            CandidateFactory = candidateFactory;
        }

        public bool TryExtract(HtmlDocument htmlDocument, out LocationCandidate? candidate)
        {
            candidate = null;
            List<string> listRegex =
            [
                @$"[""']?(?:latitude|lat|latitud)[""']?\s*(?:=|:)\s*[""']?(?<lat>{NumberPattern})[""']?[^<>]{{0,300}}?[""']?(?:longitude|lng|lon|longitud)[""']?\s*(?:=|:)\s*[""']?(?<lng>{NumberPattern})[""']?",
                @$"[""']?(?:longitude|lng|lon|longitud)[""']?\s*(?:=|:)\s*[""']?(?<lng>{NumberPattern})[""']?[^<>]{{0,300}}?[""']?(?:latitude|lat|latitud)[""']?\s*(?:=|:)\s*[""']?(?<lat>{NumberPattern})[""']?",
                @$"LatLng\s*\(\s*(?<lat>{NumberPattern})\s*,\s*(?<lng>{NumberPattern})\s*\)",
                @$"[""']?center[""']?\s*:\s*\{{[^{{}}]{{0,300}}?[""']?(?:latitude|lat)[""']?\s*:\s*[""']?(?<lat>{NumberPattern})[""']?[^{{}}]{{0,300}}?[""']?(?:longitude|lng|lon)[""']?\s*:\s*[""']?(?<lng>{NumberPattern})[""']?[^{{}}]{{0,300}}?\}}",
                @$"[""']?center[""']?\s*:\s*\{{[^{{}}]{{0,300}}?[""']?(?:longitude|lng|lon)[""']?\s*:\s*[""']?(?<lng>{NumberPattern})[""']?[^{{}}]{{0,300}}?[""']?(?:latitude|lat)[""']?\s*:\s*[""']?(?<lat>{NumberPattern})[""']?[^{{}}]{{0,300}}?\}}",
                @$"[""']?coordinates[""']?\s*:\s*\[\s*[""']?(?<lng>{NumberPattern})[""']?\s*,\s*[""']?(?<lat>{NumberPattern})[""']?\s*\]"
            ];

            string text = htmlDocument.DocumentNode.InnerHtml
                .Replace("\n", string.Empty)
                .Replace("\r", string.Empty);

            foreach (var regex in listRegex)
            {
                if (TryExtract(text, regex, LocationCandidateSources.Html, out candidate))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryExtract(
            string text,
            string regexPattern,
            string source,
            out LocationCandidate? candidate)
        {
            candidate = null;
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            var matches = regex.Matches(text);
            foreach (Match match in matches.Cast<Match>())
            {
                if (TryCreateCandidateFromNamedGroups(match, source, out candidate))
                {
                    return true;
                }

                if (TryCreateCandidateFromNumericGroups(match, source, out candidate))
                {
                    return true;
                }
            }
            return false;
        }

        private bool TryCreateCandidateFromNamedGroups(
            Match match,
            string source,
            out LocationCandidate? candidate)
        {
            candidate = null;
            var latGroup = match.Groups["lat"];
            var lngGroup = match.Groups["lng"];
            if (!latGroup.Success || !lngGroup.Success)
            {
                return false;
            }

            return CandidateFactory.TryCreate(latGroup.Value, lngGroup.Value, false, source, out candidate);
        }

        private bool TryCreateCandidateFromNumericGroups(
            Match match,
            string source,
            out LocationCandidate? candidate)
        {
            candidate = null;
            double? latitude = null;

            foreach (Group group in match.Groups.Cast<Group>())
            {
                var value = group.Value;
                if (value.StartsWith('.'))
                {
                    continue;
                }
                if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double latOrLng))
                {
                    continue;
                }
                if (latitude == null)
                {
                    latitude = latOrLng;
                }
                else if (CandidateFactory.TryCreate((double)latitude, latOrLng, false, source, out candidate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
