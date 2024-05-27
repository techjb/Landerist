using HtmlAgilityPack;
using landerist_library.Websites;
using System.Text.RegularExpressions;

namespace landerist_library.Parse.Listing
{
    public partial class UserTextInput
    {
        private static readonly HashSet<string> TagsToRemove =
        [
            //"//head",
            "//script",
            "//style",
            "//link",
            //"//header",
            //"//nav",
            //"//footer",
            //"//aside",
            //"//a",
            "//code",
            "//canvas",
            //"//meta",
            "//option",
            "//select",
            "//progress",
            "//svg",
            "//textarea",
            //"//del",
            "//button",
            //"//form[not(.//input[@id='__VIEWSTATE' or @id='__VIEWSTATEGENERATOR' or @id='__EVENTVALIDATION'])]",
            "//input",
            "//img",
            "//iframe",
            //"//*[contains(@style, 'text-decoration: line-through')]",
            //"//*[contains(@style, 'text-decoration:line-through')]"
        ];

        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static string InitXpathTagsToRemove()
        {
            return string.Join(" | ", TagsToRemove.ToList());
        }

        public static string? GetText(Page page)
        {
            return page.ResponseBodyText;

            try
            {
                var htmlDocument = page.GetHtmlDocument();
                if (htmlDocument != null)
                {
                    return GetHtml(htmlDocument);                    
                }
            }
            catch { }
            return null;
        }

        public static string? GetHtml(HtmlDocument htmlDocument)
        {
            string? text = null;
            try
            {
                RemoveNodes(htmlDocument, XpathTagsToRemove);
                //RemoveAttributes(htmlDocument);
                text = CleanHtml(htmlDocument);                
                return text;
            }
            catch { }
            return text;
        }

        private static void RemoveNodes(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }

        //private static void RemoveAttributes(HtmlDocument htmlDocument)
        //{
        //    foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//*"))
        //    {
        //        node.Attributes.RemoveAll();
        //    }
        //}

        static string CleanHtml(HtmlDocument htmlDocument)
        {
            string html = htmlDocument.DocumentNode.OuterHtml;
            html = RegexSpace().Replace(html, " ");
            html = Regex1().Replace(html, ">");
            html = Regex2().Replace(html, "<");

            return html.Trim();
        }

        //private static IEnumerable<string>? GetVisibleText(HtmlDocument htmlDocument)
        //{
        //    var visibleNodes = htmlDocument.DocumentNode.DescendantsAndSelf()
        //        .Where(n => n.NodeType == HtmlNodeType.Text)
        //           .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml))
        //           ;

        //    return visibleNodes.Select(n => n.InnerHtml.Trim());
        //}

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexSpace();
        [GeneratedRegex(@"\s*>")]
        private static partial Regex Regex1();
        [GeneratedRegex(@"<\s*")]
        private static partial Regex Regex2();
    }
}
