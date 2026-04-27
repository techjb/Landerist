using System.Text.RegularExpressions;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static partial class ListingStructuredDataValues
    {
        public static void Add(Dictionary<string, List<string>> values, string key, string? rawValue)
        {
            string value = Clean(rawValue);
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!values.TryGetValue(key, out var list))
            {
                list = [];
                values[key] = list;
            }

            if (!list.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                list.Add(value);
            }
        }

        public static string? FormatBlock(string label, Dictionary<string, List<string>> values)
        {
            if (values.Count == 0)
            {
                return null;
            }

            var pairs = values
                .Where(pair => pair.Value.Count > 0)
                .Select(pair => $"{pair.Key}={string.Join(", ", pair.Value.Take(12))}")
                .Where(pair => !string.IsNullOrWhiteSpace(pair))
                .Take(40)
                .ToList();

            if (pairs.Count == 0)
            {
                return null;
            }

            return label + ": " + string.Join("; ", pairs);
        }

        private static string Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            value = HttpUtility.HtmlDecode(value);
            value = RegexSpace().Replace(value, " ").Trim();

            const int maxValueLength = 500;
            if (value.Length > maxValueLength)
            {
                value = value[..maxValueLength].Trim();
            }

            return value;
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();
    }
}
