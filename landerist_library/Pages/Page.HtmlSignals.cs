using landerist_library.Index;
using landerist_library.Tools;

namespace landerist_library.Pages
{
    public partial class Page
    {
        public bool IsMainPage()
        {
            if (Website == null)
            {
                return false;
            }
            return Uri.Equals(Website.MainUri);
        }

        public bool ContainsMetaRobotsNoIndex()
        {
            return ContainsMetaRobots("noindex");
        }

        public bool ContainsMetaRobotsNoFollow()
        {
            return ContainsMetaRobots("nofollow");
        }

        public bool ContainsMetaRobotsNoImageIndex()
        {
            return ContainsMetaRobots("noimageindex");
        }

        public bool NotCanonical()
        {
            var canonicalUri = GetCanonicalUri();
            if (canonicalUri == null)
            {
                return false;
            }
            return !Uri.Equals(canonicalUri);
        }

        public bool RedirectToAnotherUrl()
        {
            return !string.IsNullOrEmpty(RedirectUrl);
        }

        public bool IncorrectLanguage()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var htmlNode = htmlDocument.DocumentNode.SelectSingleNode("/html");
                if (htmlNode != null)
                {
                    var lang = htmlNode.Attributes["lang"];
                    if (lang != null)
                    {
                        return !LanguageValidator.IsValidLanguageAndCountry(Website, lang.Value);
                    }
                }
            }
            return false;
        }

        public Uri? GetCanonicalUri()
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("href", "");
                    var canonicalUri = new Indexer(this).GetUri(contentAttribute);
                    return IsIgnoredCanonicalUri(canonicalUri) ? null : canonicalUri;
                }
            }
            return null;
        }

        private bool IsIgnoredCanonicalUri(Uri? canonicalUri)
        {
            if (canonicalUri is null)
            {
                return false;
            }

            return IsRemaxHost(Host) &&
                IsRemaxHost(canonicalUri.Host) &&
                canonicalUri.AbsolutePath.TrimEnd('/').Equals(RemaxInvalidCanonicalPath, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRemaxHost(string host)
        {
            return host.Equals("remax.es", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("www.remax.es", StringComparison.OrdinalIgnoreCase);
        }

        private bool ContainsMetaRobots(string content)
        {
            var htmlDocument = GetHtmlDocument();
            if (htmlDocument != null)
            {
                var node = htmlDocument.DocumentNode.SelectSingleNode("//meta[@name='robots']");
                if (node != null)
                {
                    var contentAttribute = node.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(contentAttribute))
                    {
                        var contents = contentAttribute.Split(',');
                        foreach (var item in contents)
                        {
                            if (item.Equals(content) || item.Equals("none"))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
