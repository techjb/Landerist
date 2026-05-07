using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingJsonLdStructuredDataExtractor
    {
        public static string? Extract(HtmlDocument htmlDocument)
        {
            var scripts = htmlDocument.DocumentNode.SelectNodes(
                "//script[contains(translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'application/ld+json')]");

            if (scripts == null)
            {
                return null;
            }

            List<string> blocks = [];
            foreach (var script in scripts)
            {
                string json = HttpUtility.HtmlDecode(script.InnerText)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }

                try
                {
                    JToken.Parse(json);
                    blocks.Add("JSON-LD: " + json);
                }
                catch
                {
                }
            }

            return blocks.Count == 0
                ? null
                : string.Join(Environment.NewLine, blocks.Distinct(StringComparer.Ordinal));
        }
    }
}
