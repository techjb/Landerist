using HtmlAgilityPack;
using landerist_library.Pages;
using Newtonsoft.Json;

namespace landerist_library.Parse.PageTypeParser
{
    public class ListingSimilarity
    {
        //private static readonly HashSet<string> Tags = [];
        private static readonly List<string> TagsToRemove =
        [
            "//head",
            "//script",
            "//style",
            //"//header",
            //"//nav",
            //"//footer",
            "//aside",
            "//a",
            "//code",
            "//canvas",
            "//meta",
            "//option",
            "//select",
            "//progress",
            "//svg",
            "//textarea",
            "//del",
            //"//button",
            //"//form[not(.//input[@id='__VIEWSTATE' or @id='__VIEWSTATEGENERATOR' or @id='__EVENTVALIDATION'])]",
            //"//input",
            //"//*[contains(@style, 'text-decoration: line-through')]",
            //"//*[contains(@style, 'text-decoration:line-through')]"
            "//ul",
            "//ol",
            "//li",
            "//img",
            "//i",
            "//b",
            "//u",
            //"//s",
            "//em",
            "//br",
            "//small",
            "//strong",
            "//link",
            "//iframe",
            "//figure",
            "//sub",
            "//sup",
            "//ins",
            "//strike",
            "//del",
            "//mark",
            "//font",
            "//code",
            "//big",
            "//tt",
            "//pre",
            "//blockquote",
            "//q",
            "//abbr",
            "//cite",
            "//dfn",
            "//address",
        ];

        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static string InitXpathTagsToRemove()
        {
            return string.Join(" | ", TagsToRemove);
        }

        public static bool HtmlNotSimilarToListing(Page page)
        {
            if (page?.Website?.ListingExampleNodeSet == null)
            {
                return false;
            }

            var htmlDocument = page.GetHtmlDocument();
            if (htmlDocument?.DocumentNode == null)
            {
                return false;
            }

            try
            {
                var nodeSet1 = JsonConvert.DeserializeObject<HashSet<string>>(page.Website.ListingExampleNodeSet);
                if (nodeSet1 != null)
                {
                    var nodeSet2 = GetNodeSet(htmlDocument);

                    double similarity = JaccardCompare(nodeSet1, nodeSet2);
                    return similarity < Configuration.Config.MINIMUM_PERCENTAGE_TO_BE_SIMILAR_PAGE;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("ListingSimilarity HtmlNotSimilarToListing", exception);
            }

            return false;
        }

        //private static double JaccardCompare(HtmlDocument htmlDocument1, HtmlDocument htmlDocument2)
        //{
        //    HashSet<string> nodeSet1 = [];
        //    HashSet<string> nodeSet2 = [];
        //
        //    BuildNodeSet(htmlDocument1.DocumentNode, nodeSet1);
        //    BuildNodeSet(htmlDocument2.DocumentNode, nodeSet2);
        //
        //    return JaccardCompare(nodeSet1, nodeSet2);
        //}

        //private static double JaccardCompare(HashSet<string> nodeSet1, HtmlDocument htmlDocument)
        //{
        //    HashSet<string> nodeSet2 = [];
        //
        //    BuildNodeSet(htmlDocument.DocumentNode, nodeSet2);
        //    return JaccardCompare(nodeSet1 , nodeSet2);
        //}

        private static double JaccardCompare(HashSet<string> nodeSet1, HashSet<string> nodeSet2)
        {
            if (nodeSet1 == null || nodeSet2 == null)
            {
                return 0d;
            }

            var union = new HashSet<string>(nodeSet1);
            union.UnionWith(nodeSet2);

            if (union.Count == 0)
            {
                return 1d;
            }

            var intersection = new HashSet<string>(nodeSet1);
            intersection.IntersectWith(nodeSet2);

            return (double)intersection.Count / union.Count;
        }

        public static string GetNodeSetSerialized(HtmlDocument htmlDocument)
        {
            var nodeSet = GetNodeSet(htmlDocument);
            return JsonConvert.SerializeObject(nodeSet, Formatting.Indented);
        }

        private static HashSet<string> GetNodeSet(HtmlDocument htmlDocument)
        {
            if (htmlDocument?.DocumentNode == null)
            {
                return [];
            }

            var rootNode = htmlDocument.DocumentNode.CloneNode(deep: true);

            RemoveTextContent(rootNode);
            RemoveTags(rootNode);

            HashSet<string> nodeSet = [];
            BuildNodeSet(rootNode, nodeSet);
            return nodeSet;
        }

        public static void RemoveTextContent(HtmlNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.NodeType == HtmlNodeType.Text)
            {
                node.Remove();
                return;
            }

            foreach (var child in node.ChildNodes.ToArray())
            {
                RemoveTextContent(child);
            }
        }

        private static void RemoveTags(HtmlNode rootNode)
        {
            if (rootNode == null)
            {
                return;
            }

            var htmlNodeCollection = rootNode.SelectNodes(XpathTagsToRemove);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }

        private static void BuildNodeSet(HtmlNode node, HashSet<string> nodeSet)
        {
            if (node == null)
            {
                return;
            }

            string nodeRepresentation = node.NodeType == HtmlNodeType.Element
                ? node.Name.ToLowerInvariant()
                : node.NodeType.ToString();

            nodeSet.Add(nodeRepresentation);
            foreach (HtmlNode child in node.ChildNodes)
            {
                BuildNodeSet(child, nodeSet);
            }
        }
    }
}
