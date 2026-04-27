using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingMetaStructuredDataExtractor
    {
        private static readonly HashSet<string> MetaNamesToExtract = new(StringComparer.OrdinalIgnoreCase)
        {
            "description",
            "name",
            "title",
            "image",
            "url",
            "price",
            "priceCurrency",
            "latitude",
            "longitude",
            "og:title",
            "og:description",
            "og:image",
            "og:image:url",
            "og:image:secure_url",
            "og:url",
            "twitter:title",
            "twitter:description",
            "twitter:image",
            "twitter:image:src",
            "product:price:amount",
            "product:price:currency",
            "place:location:latitude",
            "place:location:longitude",
            "geo.position",
            "geo.placename",
            "geo.region",
        };

        public static string? Extract(HtmlDocument htmlDocument)
        {
            var metas = htmlDocument.DocumentNode.SelectNodes("//meta[@content]");
            if (metas == null)
            {
                return null;
            }

            Dictionary<string, List<string>> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (var meta in metas)
            {
                string name = GetMetaName(meta);
                if (string.IsNullOrWhiteSpace(name) || !MetaNamesToExtract.Contains(name))
                {
                    continue;
                }

                ListingStructuredDataValues.Add(values, name, meta.GetAttributeValue("content", string.Empty));
            }

            return ListingStructuredDataValues.FormatBlock("META", values);
        }

        private static string GetMetaName(HtmlNode meta)
        {
            string name = meta.GetAttributeValue("property", string.Empty);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name.Trim();
            }

            name = meta.GetAttributeValue("name", string.Empty);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name.Trim();
            }

            return meta.GetAttributeValue("itemprop", string.Empty).Trim();
        }
    }
}
