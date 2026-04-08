using HtmlAgilityPack;
using landerist_library.Websites;

namespace landerist_library.Index
{
    public class LinkAlternateIndexer(Page page) : Indexer(page)
    {
        public void Insert()
        {
            var htmlDocument = Page.GetHtmlDocument();
            if (htmlDocument == null)
            {
                return;
            }

            try
            {
                var linkNodes = htmlDocument.DocumentNode.SelectNodes("//link[@rel]");
                if (linkNodes == null)
                {
                    return;
                }

                foreach (var linkNode in linkNodes)
                {
                    var rel = linkNode.GetAttributeValue("rel", string.Empty);
                    if (rel.Contains("alternate", StringComparison.OrdinalIgnoreCase))
                    {
                        Insert(linkNode);
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LinkAlternateIndexer InsertLinksAlternate", Page.Uri, exception);
            }
        }

        private void Insert(HtmlNode? htmlNode)
        {
            if (htmlNode == null)
            {
                return;
            }

            try
            {
                var hreflang = htmlNode.GetAttributeValue("hreflang", string.Empty);
                if (!LanguageValidator.IsValidLanguageAndCountry(Page.Website, hreflang))
                {
                    return;
                }

                var href = htmlNode.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrWhiteSpace(href))
                {
                    return;
                }

                Insert(href);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("LinkAlternateIndexer InsertLinkAlternateNode", Page.Uri, exception);
            }
        }
    }
}
