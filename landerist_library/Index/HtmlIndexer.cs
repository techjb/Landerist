using landerist_library.Websites;

namespace landerist_library.Index
{
    public class HtmlIndexer: Indexer
    {
        public HtmlIndexer(Page page): base(page) { }

        public void InsertHyperlinks()
        {
            if (Page.HtmlDocument == null)
            {
                return;
            }
            try
            {
                var urls = Page.HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrWhiteSpace(href))
                   .ToList();

                if (urls != null)
                {
                    InsertUrls(urls);
                }
            }
            catch (Exception ecception)
            {
                Logs.Log.WriteLogErrors(Page.Uri, ecception);
            }
        }
    }
}
