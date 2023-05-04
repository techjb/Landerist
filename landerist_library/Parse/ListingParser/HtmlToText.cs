using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser
{
    public class HtmlToText
    {

        public static string GetText(HtmlDocument htmlDocument)
        {
            try
            {
                htmlDocument = RemoveNodes(htmlDocument);
                var visibletText = GetVisibleText(htmlDocument);
                return CleanText(visibletText);
            }
            catch { }
            return string.Empty;
        }


        private static HtmlDocument RemoveNodes(HtmlDocument htmlDocument)
        {
            var xPath =
                "//script | //nav | //footer | //style | //head | " +
                "//a | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = htmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
            return htmlDocument;
        }

        private static IEnumerable<string>? GetVisibleText(HtmlDocument htmlDocument)
        {
            var visibleNodes = htmlDocument.DocumentNode.DescendantsAndSelf().Where(
                   n => n.NodeType == HtmlNodeType.Text)
                   .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml));

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
            string text = string.Join(Environment.NewLine, cleanedLines);
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
