using HtmlAgilityPack;
using System.Text;

namespace landerist_library.Tools
{
    public class HtmlToText
    {

        private static readonly List<string> ListTextToRemoveNode = new()
        {
            "cookie",  "javascript", "navegador", "browser"
        };

        private const string TagsToRemove =
                "//script | //nav | //footer | //style | //head | " +
                "//a | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea | //del | //aside";

        public static string GetText(HtmlDocument htmlDocument)
        {
            try
            {
                RemoveNodesWithTags(htmlDocument);
                RemoveNodesWithTexts(htmlDocument);
                var visibleText = GetVisibleText(htmlDocument);
                return CleanText(visibleText);
            }
            catch { }
            return string.Empty;
        }


        private static void RemoveNodesWithTags(HtmlDocument htmlDocument)
        {
            var nodesToRemove = htmlDocument.DocumentNode.SelectNodes(TagsToRemove);
            if (nodesToRemove != null)
            {
                RemoveNodes(nodesToRemove.ToList());                
            }
        }

        private static void RemoveNodesWithTexts(HtmlDocument htmlDocument)
        {
            var queryBuilder = new StringBuilder("//*[text()[");
            foreach (var word in ListTextToRemoveNode)
            {
                queryBuilder.Append($"contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{word.ToLower()}') or ");
            }
            queryBuilder.Length -= 4;
            queryBuilder.Append("]]");

            var nodesToRemove = htmlDocument.DocumentNode.SelectNodes(queryBuilder.ToString());

            if (nodesToRemove != null)
            {
                RemoveNodes(nodesToRemove.ToList());
            }            
        }
        
        private static void RemoveNodes(List<HtmlNode> nodesToRemove)
        {
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private static IEnumerable<string>? GetVisibleText(HtmlDocument htmlDocument)
        {
            var visibleNodes = htmlDocument.DocumentNode.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text)
                   .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml))
                   ;

            return visibleNodes.Select(n => n.InnerHtml.Trim());
        }

        private static string CleanText(IEnumerable<string>? lines)
        {
            List<string> cleanedLines = new();
            if (lines == null)
            {
                return string.Empty;
            }
            foreach (var line in lines)
            {
                string decodedLine = HtmlEntity.DeEntitize(line).Trim();
                if (string.IsNullOrEmpty(decodedLine))
                {
                    continue;
                }
                if (IsSymbol(decodedLine))
                {
                    continue;
                }
                cleanedLines.Add(decodedLine);
            }
            string text = string.Join(" ", cleanedLines);
            text = Strings.Clean(text);
            return text;
        }

        private static bool IsSymbol(string input)
        {
            foreach (char c in input)
            {
                if (!char.IsPunctuation(c) && !char.IsSymbol(c))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
