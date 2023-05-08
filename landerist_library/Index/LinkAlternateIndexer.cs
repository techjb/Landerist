using HtmlAgilityPack;
using landerist_library.Websites;

namespace landerist_library.Index
{
    public class LinkAlternateIndexer : Indexer
    {
        public LinkAlternateIndexer(Page page) : base(page) { }

        public void InsertLinksAlternate()
        {
            if (Page.HtmlDocument == null)
            {
                return;
            }
            try
            {
                var linkNodes = Page.HtmlDocument.DocumentNode.SelectNodes("//link[@rel='alternate']");
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
                Logs.Log.WriteLogErrors(Page.Uri, ecception);
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
                    InsertUrl(href);
                }
            }
            catch { }
        }


    }
}
