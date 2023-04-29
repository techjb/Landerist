using HtmlAgilityPack;

namespace landerist_library.Parse.ListingParser
{
    public class HtmlToText
    {
        private readonly HtmlDocument HtmlDocument;

        private string HtmlText = string.Empty;

        public HtmlToText(HtmlDocument htmlDocument)
        {
            HtmlDocument = htmlDocument;
        }

        public string GetText()
        {
            try
            {
                RemoveNodes();
                GetVisibleText();
            }
            catch { }
            return HtmlText.Trim();
        }


        private void RemoveNodes()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var xPath =
                "//script | //nav | //footer | //style | //head | " +
                "//form | //a | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea | //del";

            var nodesToRemove = HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private void GetVisibleText()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var visibleNodes = HtmlDocument.DocumentNode.DescendantsAndSelf().Where(
                   n => n.NodeType == HtmlNodeType.Text)
                   .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml))
                   .Where(n => !n.InnerHtml.Trim().ToLower().Equals("&nbsp;"))
                   ;

            var visibleText = visibleNodes.Select(n => n.InnerHtml.Trim());

            HtmlText = string.Join(Environment.NewLine, visibleText);
        }
    }
}
