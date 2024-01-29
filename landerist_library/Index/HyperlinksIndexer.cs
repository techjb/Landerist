using HtmlAgilityPack;
using landerist_library.Websites;
using System.Text.RegularExpressions;

namespace landerist_library.Index
{
    public class HyperlinksIndexer(Page page) : Indexer(page)
    {
        public void Insert()
        {
            if (Page.HtmlDocument == null)
            {
                return;
            }
            try
            {
                //var nodes = Page.HtmlDocument.DocumentNode.SelectNodes("//a");
                //if (nodes == null)
                //{
                //    return;
                //}
                //List<string?> urls = [];
                //foreach ( var node in nodes )
                //{
                //    if (IsHoneypotTrap(node))
                //    {
                //        continue;
                //    }

                //    string rel = node.GetAttributeValue("rel", null);
                //    if(rel != null && rel.ToLower().Equals("nofollow"))
                //    {
                //        continue;
                //    }

                //    string href = node.GetAttributeValue("href", null);                    
                //    if (string.IsNullOrEmpty(href))
                //    {
                //        continue;
                //    }
                //    urls.Add(href);
                //}

                var urls = Page.HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Where(a => !IsHoneypotTrap(a))
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrEmpty(href))
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

        static bool IsHoneypotTrap(HtmlNode link)
        {
            try
            {
                var style = link.GetAttributeValue("style", string.Empty);
                if (!string.IsNullOrEmpty(style))
                {
                    var regex = new Regex(@"display\s*:\s*none|visibility\s*:\s*hidden", RegexOptions.IgnoreCase);
                    return regex.IsMatch(style);
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
