using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingEmptyElementRemover
    {
        private static readonly HashSet<string> ElementsToPreserve =
        [
            "html",
            "body",
            "img",
            "source",
            "picture",
            "br",
            "hr",
        ];

        private static readonly HashSet<string> InformativeAttributes = new(StringComparer.OrdinalIgnoreCase)
        {
            "href",
            "src",
            "srcset",
            "alt",
            "title",
            "content",
            "itemprop",
            "property",
            "name",
            "datetime",
            "aria-label",
            "data-landerist-structured-data",
        };

        public static void Remove(HtmlDocument htmlDocument)
        {
            bool removed;
            do
            {
                removed = false;
                var emptyNodes = htmlDocument.DocumentNode
                    .Descendants()
                    .Where(IsRemovableEmptyElement)
                    .OrderByDescending(node => node.Ancestors().Count())
                    .ToList();

                foreach (var node in emptyNodes)
                {
                    node.Remove();
                    removed = true;
                }
            }
            while (removed);
        }

        private static bool IsRemovableEmptyElement(HtmlNode node)
        {
            if (node.NodeType != HtmlNodeType.Element || node.ParentNode == null)
            {
                return false;
            }

            if (ElementsToPreserve.Contains(node.Name))
            {
                return false;
            }

            if (HasInformativeAttribute(node))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(node.InnerText))
            {
                return false;
            }

            return !node.ChildNodes.Any(child => child.NodeType == HtmlNodeType.Element);
        }

        private static bool HasInformativeAttribute(HtmlNode node)
        {
            if (!node.HasAttributes)
            {
                return false;
            }

            return node.Attributes.Any(attribute =>
                InformativeAttributes.Contains(attribute.Name) ||
                attribute.Name.StartsWith("data-", StringComparison.OrdinalIgnoreCase));
        }
    }
}
