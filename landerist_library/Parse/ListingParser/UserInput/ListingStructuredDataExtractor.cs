using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingStructuredDataExtractor
    {
        public static string? Extract(HtmlDocument htmlDocument)
        {
            List<string> blocks = [];
            AddIfNotEmpty(blocks, ListingJsonLdStructuredDataExtractor.Extract(htmlDocument));
            AddIfNotEmpty(blocks, ListingMetaStructuredDataExtractor.Extract(htmlDocument));

            if (blocks.Count == 0)
            {
                return null;
            }

            return string.Join(Environment.NewLine, blocks.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static void AddIfNotEmpty(List<string> blocks, string? block)
        {
            if (!string.IsNullOrWhiteSpace(block))
            {
                blocks.Add(block);
            }
        }
    }
}
