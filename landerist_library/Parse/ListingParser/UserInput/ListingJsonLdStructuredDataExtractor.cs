using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingJsonLdStructuredDataExtractor
    {
        private static readonly HashSet<string> JsonLdPropertiesToExtract = new(StringComparer.OrdinalIgnoreCase)
        {
            "@type",
            "type",
            "name",
            "headline",
            "title",
            "description",
            "url",
            "mainEntityOfPage",
            "image",
            "images",
            "photo",
            "photos",
            "thumbnail",
            "thumbnailUrl",
            "contentUrl",
            "address",
            "streetAddress",
            "addressLocality",
            "addressRegion",
            "postalCode",
            "addressCountry",
            "geo",
            "latitude",
            "longitude",
            "price",
            "priceCurrency",
            "lowPrice",
            "highPrice",
            "availability",
            "floorSize",
            "numberOfRooms",
            "numberOfBedrooms",
            "numberOfBathroomsTotal",
            "rooms",
            "bedrooms",
            "bathrooms",
            "telephone",
            "email",
            "seller",
            "provider",
            "broker",
            "brand",
            "datePosted",
            "sku",
            "productID",
            "identifier",
        };

        public static string? Extract(HtmlDocument htmlDocument)
        {
            var scripts = htmlDocument.DocumentNode.SelectNodes(
                "//script[contains(translate(@type, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'application/ld+json')]");

            if (scripts == null)
            {
                return null;
            }

            Dictionary<string, List<string>> values = new(StringComparer.OrdinalIgnoreCase);
            foreach (var script in scripts)
            {
                string json = HttpUtility.HtmlDecode(script.InnerText)?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(json))
                {
                    continue;
                }

                try
                {
                    CollectStructuredData(JToken.Parse(json), values);
                }
                catch
                {
                }
            }

            return ListingStructuredDataValues.FormatBlock("JSON-LD", values);
        }

        private static void CollectStructuredData(JToken token, Dictionary<string, List<string>> values)
        {
            if (token is JObject jsonObject)
            {
                foreach (var property in jsonObject.Properties())
                {
                    string propertyName = property.Name.TrimStart('@');
                    if (JsonLdPropertiesToExtract.Contains(property.Name) || JsonLdPropertiesToExtract.Contains(propertyName))
                    {
                        foreach (string value in FlattenStructuredValues(property.Value).Take(12))
                        {
                            ListingStructuredDataValues.Add(values, propertyName, value);
                        }
                    }

                    CollectStructuredData(property.Value, values);
                }

                return;
            }

            if (token is JArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    CollectStructuredData(item, values);
                }
            }
        }

        private static IEnumerable<string> FlattenStructuredValues(JToken token)
        {
            if (token is JValue jsonValue)
            {
                string? text = jsonValue.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    yield return text;
                }

                yield break;
            }

            if (token is JArray array)
            {
                foreach (var item in array)
                {
                    foreach (string flattenedValue in FlattenStructuredValues(item))
                    {
                        yield return flattenedValue;
                    }
                }

                yield break;
            }

            if (token is JObject jsonObject)
            {
                foreach (var property in jsonObject.Properties())
                {
                    if (property.Value is JValue)
                    {
                        foreach (string flattenedValue in FlattenStructuredValues(property.Value))
                        {
                            yield return $"{property.Name}:{flattenedValue}";
                        }
                    }
                }
            }
        }
    }
}
