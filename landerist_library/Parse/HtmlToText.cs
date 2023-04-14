using HtmlAgilityPack;

namespace landerist_library.Parse
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
                RemoveResponseBodyNodes();
                SetResponseBodyTextVisible();
            }
            catch { }
            return HtmlText.Trim();
        }


        private void RemoveResponseBodyNodes()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var xPath =
                "//script | //nav | //footer | //style | //head | " +
                "//form | //a | //code | //canvas | //input | //meta | //option | " +
                "//select | //progress | //svg | //textarea";

            var nodesToRemove = HtmlDocument.DocumentNode.SelectNodes(xPath).ToList();
            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private void SetResponseBodyTextVisible()
        {
            if (HtmlDocument == null)
            {
                return;
            }
            var visibleNodes = HtmlDocument.DocumentNode.DescendantsAndSelf().Where(
                   n => n.NodeType == HtmlNodeType.Text)
                   .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml));

            var visibleText = visibleNodes.Select(n => n.InnerHtml.Trim());
            HtmlText = string.Join(Environment.NewLine, visibleText);
        }
    }
}
