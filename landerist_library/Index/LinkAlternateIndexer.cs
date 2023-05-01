using HtmlAgilityPack;
using landerist_library.Websites;

namespace landerist_library.Index
{
    public class LinkAlternateIndexer : Indexer
    {
        public LinkAlternateIndexer(Page page) : base(page) { }

        public void Insert()
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
                if (hreflang.StartsWith(Page.Website.Language))
                {
                    var hrefAttr = htmlNode.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(hrefAttr))
                    {
                        InsertUrl(hrefAttr);
                    }
                }
            }
            catch { }
        }
    }
}
