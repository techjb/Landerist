using HtmlAgilityPack;
using landerist_library.Websites;
using SimMetrics.Net.Metric;
using System.Net;

namespace landerist_library.Parse.PageTypeParser
{
    public class ListingSimilarity
    {
        private static readonly HashSet<string> Tags = [];
        private static readonly List<string> TagsToRemove =
        [
            "//head",
            "//script",
            "//style",
            //"//header",
            //"//nav",
            "//footer",
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
            "//form[not(.//input[@id='__VIEWSTATE' or @id='__VIEWSTATEGENERATOR' or @id='__EVENTVALIDATION'])]",
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
            "//s",
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
            return string.Join(" | ", TagsToRemove.ToList());
        }

        public static async Task<string?> GetListingUrlHtml(Website website)
        {
            if (website.ListingUri == null)
            {
                return null;
            }
            var httpClient = new HttpClient();
            try
            {
                var html = await httpClient.GetStringAsync(website.ListingUri);
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                RemoveTextContent(htmlDoc.DocumentNode);
                return htmlDoc.DocumentNode.OuterHtml;
            }
            catch
            {
                return null;
            }
        }

        private static void RemoveTextContent(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Text)
            {
                node.Remove();
            }
            else
            {
                foreach (var child in node.ChildNodes.ToArray())
                {
                    RemoveTextContent(child);
                }
            }
        }


        public static void Test(string url1, string url2)
        {
            var htmlDocument1 = LoadUrlToHtmlDocument(url1);
            var htmlDocument2 = LoadUrlToHtmlDocument(url2);

            Clean(htmlDocument1);
            Clean(htmlDocument2);

            //SimpleCompare(htmlDocument1, htmlDocument2);
            JacardCompare(htmlDocument1, htmlDocument2);
            //LevenshteinCompare(htmlDocument1, htmlDocument2);
        }

        private static HtmlDocument LoadUrlToHtmlDocument(string url)
        {
            var htmlDocument = new HtmlDocument();
            try
            {
                using WebClient client = new();
                string pageContent = client.DownloadString(url);

                htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(pageContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading URL to HtmlDocument: " + ex.Message);
            }
            return htmlDocument;
        }

        private static void LevenshteinCompare(HtmlDocument htmlDocument1, HtmlDocument htmlDocument2)
        {
            string text1 = ConvertDomToStructuralString(htmlDocument1.DocumentNode);
            string text2 = ConvertDomToStructuralString(htmlDocument2.DocumentNode);

            var cosine = new Levenstein();
            var similarity = cosine.GetSimilarity(text1, text2);
            Console.WriteLine($"GetSimilarity: {similarity}");
        }
        private static string ConvertDomToStructuralString(HtmlNode node)
        {
            if (node.NodeType == HtmlNodeType.Document)
            {
                return string.Join("", node.ChildNodes.Select(n => ConvertDomToStructuralString(n)));
            }
            if (node.NodeType == HtmlNodeType.Element)
            {
                var childStructure = string.Join("", node.ChildNodes.Select(n => ConvertDomToStructuralString(n)));
                return $"<{node.Name}>{childStructure}</{node.Name}>";
            }
            return "";
        }



        private static void JacardCompare(HtmlDocument htmlDocument1, HtmlDocument htmlDocument2)
        {
            HashSet<string> nodeSet1 = [];
            HashSet<string> nodeSet2 = [];

            BuildNodeSet(htmlDocument1.DocumentNode, nodeSet1);
            BuildNodeSet(htmlDocument2.DocumentNode, nodeSet2);

            var intersection = new HashSet<string>(nodeSet1);
            intersection.IntersectWith(nodeSet2);

            var union = new HashSet<string>(nodeSet1);
            union.UnionWith(nodeSet2);

            var except = new HashSet<string>(nodeSet1);
            except.ExceptWith(nodeSet2);

            double jaccardSimilarity = (double)intersection.Count / union.Count;
            Console.WriteLine($"{(double)intersection.Count }/{union.Count} = {jaccardSimilarity:P2}");

            Console.WriteLine(" -- EXCEPT -- ");
            foreach (var tag in except)
            {
                Console.WriteLine(tag);
            }
        }

        static void BuildNodeSet(HtmlNode node, HashSet<string> nodeSet)
        {
            string nodeRepresentation =
                node.NodeType == HtmlNodeType.Element ?
                node.Name.ToLower() :
                node.NodeType.ToString();

            nodeSet.Add(nodeRepresentation);
            foreach (HtmlNode child in node.ChildNodes)
            {
                BuildNodeSet(child, nodeSet);
            }
        }

        private static void Clean(HtmlDocument htmlDocument)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(XpathTagsToRemove);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }


        private static void SimpleCompare(HtmlDocument htmlDocumentListing, HtmlDocument htmlDocumentTest)
        {
            int nodesListing = htmlDocumentListing.DocumentNode.SelectNodes("//*").Count;
            int nodesTest = htmlDocumentTest.DocumentNode.SelectNodes("//*").Count;

            Console.WriteLine(nodesListing + " " + nodesTest);

            int matches = 0, total = 0;

            Compare(htmlDocumentListing.DocumentNode, htmlDocumentTest.DocumentNode, ref matches, ref total);
            foreach(var tag in Tags)
            {
                Console.WriteLine(tag);
            }

            double similarityPercentage = total > 0 ? (double)matches / total * 100 : 0;
            Console.WriteLine($"Similarity: {similarityPercentage}%");
        }
        

        private static void Compare(HtmlNode node1, HtmlNode node2, ref int matches, ref int total)
        {
            total++;
            matches += CompareNodeTypes(node1, node2);

            int minCount = Math.Min(node1.ChildNodes.Count, node2.ChildNodes.Count);
            for (int i = 0; i < minCount; i++)
            {
                var node1ChildNode = node1.ChildNodes[i];
                var node2ChildNode = node2.ChildNodes[i];
                Compare(node1ChildNode, node2ChildNode, ref matches, ref total);
            }
        }

        private static int CompareNodeTypes(HtmlNode node1, HtmlNode node2)
        {
            if (node1.NodeType != node2.NodeType)
            {
                return 0;
            }
            if (node1.NodeType == HtmlNodeType.Element && node2.NodeType == HtmlNodeType.Element)
            {
                Tags.Add(node1.Name);                
                return node1.Name.Equals(node2.Name, StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            }
            return 1;
        }
    }
}
