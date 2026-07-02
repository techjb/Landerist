using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingJsonLdStructuredDataExtractor
    {
        private static readonly HashSet<string> AdvertiserTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Corporation",
            "ContactPoint",
            "LocalBusiness",
            "Organization",
            "Person",
            "RealEstateAgent",
            "RealEstateBusiness",
        };

        private static readonly HashSet<string> AdvertiserRelationshipProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "agent",
            "author",
            "brand",
            "broker",
            "contactPoint",
            "creator",
            "employee",
            "offeredBy",
            "organizer",
            "provider",
            "publisher",
            "seller",
            "worksFor",
        };

        private static readonly HashSet<string> LocationProperties = new(StringComparer.OrdinalIgnoreCase)
        {
            "address",
            "geo",
            "lat",
            "latitude",
            "lng",
            "location",
            "longitude",
        };

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
                    var token = JToken.Parse(json);
                    RemoveAdvertiserLocationData(token);
                    blocks.Add("JSON-LD: " + token.ToString(Formatting.None));
                }
                catch
                {
                }
            }

            return blocks.Count == 0
                ? null
                : string.Join(Environment.NewLine, blocks.Distinct(StringComparer.Ordinal));
        }

        private static void RemoveAdvertiserLocationData(JToken token, string? propertyName = null)
        {
            if (token is JObject obj)
            {
                if (IsAdvertiserObject(obj, propertyName))
                {
                    RemoveDirectLocationProperties(obj);
                }

                foreach (var property in obj.Properties().ToList())
                {
                    if (AdvertiserRelationshipProperties.Contains(property.Name))
                    {
                        RemoveLocationPropertiesDeep(property.Value);
                    }

                    RemoveAdvertiserLocationData(property.Value, property.Name);
                }

                return;
            }

            if (token is JArray array)
            {
                foreach (var child in array)
                {
                    RemoveAdvertiserLocationData(child, propertyName);
                }
            }
        }

        private static void RemoveLocationPropertiesDeep(JToken token)
        {
            if (token is JObject obj)
            {
                RemoveDirectLocationProperties(obj);
                foreach (var property in obj.Properties().ToList())
                {
                    RemoveLocationPropertiesDeep(property.Value);
                }

                return;
            }

            if (token is JArray array)
            {
                foreach (var child in array)
                {
                    RemoveLocationPropertiesDeep(child);
                }
            }
        }

        private static void RemoveDirectLocationProperties(JObject obj)
        {
            foreach (var property in obj.Properties().Where(property => LocationProperties.Contains(property.Name)).ToList())
            {
                property.Remove();
            }
        }

        private static bool IsAdvertiserObject(JObject obj, string? propertyName)
        {
            return (!string.IsNullOrWhiteSpace(propertyName) &&
                    AdvertiserRelationshipProperties.Contains(propertyName)) ||
                IsAdvertiserType(obj["@type"]);
        }

        private static bool IsAdvertiserType(JToken? typeToken)
        {
            if (typeToken == null)
            {
                return false;
            }

            if (typeToken is JArray array)
            {
                return array.Any(IsAdvertiserType);
            }

            if (typeToken is JValue value && value.Value != null)
            {
                return AdvertiserTypes.Contains(GetTypeName(value.Value.ToString() ?? string.Empty));
            }

            return false;
        }

        private static string GetTypeName(string type)
        {
            type = type.Trim();
            int slashIndex = type.LastIndexOfAny(['/', '#']);
            if (slashIndex >= 0 && slashIndex < type.Length - 1)
            {
                type = type[(slashIndex + 1)..];
            }

            int colonIndex = type.LastIndexOf(':');
            if (colonIndex >= 0 && colonIndex < type.Length - 1)
            {
                type = type[(colonIndex + 1)..];
            }

            return type;
        }
    }
}
