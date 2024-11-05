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
                var linkNodes = htmlDocument.DocumentNode.SelectNodes("//link[@rel='alternate']");
                if (linkNodes == null)
                {
                    return;
                }
                foreach (var linkNode in linkNodes)
                {
                    Insert(linkNode);
                }
            }
            catch (Exception ecception)
            {
                Logs.Log.WriteError("LinkAlternateIndexer InsertLinksAlternate", Page.Uri, ecception);
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
                if (LanguageValidator.IsValidLanguageAndCountry(Page.Website, hreflang))
                {
                    var href = htmlNode.GetAttributeValue("href", string.Empty);
                    Insert(href);
                }
            }
            catch { }
        }
    }
}
