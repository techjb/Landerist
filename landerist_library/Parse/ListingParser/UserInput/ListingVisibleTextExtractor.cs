using HtmlAgilityPack;
using System.Text;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingVisibleTextExtractor
    {
        public static string GetText(HtmlDocument htmlDocument)
        {
            var stringBuilder = new StringBuilder();
            foreach (var node in htmlDocument.DocumentNode.DescendantsAndSelf())
            {
                if (node.NodeType != HtmlNodeType.Text)
                {
                    continue;
                }

                if (ListingHiddenContentRemover.HasHiddenAncestor(node))
                {
                    continue;
                }

                string innerText = node.InnerText;
                if (!string.IsNullOrWhiteSpace(innerText))
                {
                    stringBuilder.AppendLine(innerText.Trim());
                }
            }

            return stringBuilder.ToString();
        }
    }
}
