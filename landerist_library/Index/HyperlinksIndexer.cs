using HtmlAgilityPack;
using landerist_library.Pages;
using System.Text.RegularExpressions;

namespace landerist_library.Index
{
    public partial class HyperlinksIndexer(Page page) : Indexer(page)
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

                // todo: select all nodes: Page.HtmlDocument.DocumentNode.SelectNodes("//a");
                var urls = htmlDocument.DocumentNode
                    .Descendants("a")
                    .Where(a => !HasNoFollow(a))
                    .Where(a => !IsHoneypotTrap(a))
                    .Select(a => a.GetAttributeValue("href", string.Empty))
                    .Where(href => !string.IsNullOrWhiteSpace(href))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Cast<string?>()
                    .ToList();

                if (urls.Count > 0)
                {
                    Insert(urls);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("HyperlinksIndexer Insert", Page.Uri, exception);
            }
        }

        private static bool HasNoFollow(HtmlNode link)
        {
            var rel = link.GetAttributeValue("rel", string.Empty);
            if (string.IsNullOrWhiteSpace(rel))
            {
                return false;
            }

            return rel
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(token => token.Equals("nofollow", StringComparison.OrdinalIgnoreCase));
        }

        static bool IsHoneypotTrap(HtmlNode link)
        {
            try
            {
                var style = link.GetAttributeValue("style", string.Empty);
                if (!string.IsNullOrEmpty(style))
                {
                    var regex = DisplayNoneOrVisibilityHidden();
                    return regex.IsMatch(style);
                }
            }
            catch
            {
            }

            return false;
        }

        [GeneratedRegex(@"display\s*:\s*none|visibility\s*:\s*hidden", RegexOptions.IgnoreCase, "es-ES")]
        private static partial Regex DisplayNoneOrVisibilityHidden();
    }
}
