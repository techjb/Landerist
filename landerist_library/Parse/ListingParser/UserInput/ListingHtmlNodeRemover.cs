using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser.UserInput
{
    internal static class ListingHtmlNodeRemover
    {
        private static readonly HashSet<string> BaseNoiseTags =
        [
            "//script",
            "//style",
            "//link",
            "//code",
            "//canvas",
            "//option",
            "//select",
            "//progress",
            "//svg",
            "//textarea",
            "//button",
            "//input",
            "//iframe",
            "//audio",
            "//video",
            "//map",
            "//area",
            "//embed",
            "//object",
            "//param",
        ];

        private static readonly string BaseNoiseXPath = string.Join(" | ", BaseNoiseTags);

        public static void RemoveBaseNoise(HtmlDocument htmlDocument)
        {
            Remove(htmlDocument, BaseNoiseXPath);
        }

        public static void Remove(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection == null)
            {
                return;
            }

            List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }
    }
}
