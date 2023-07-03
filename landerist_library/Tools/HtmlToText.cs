using HtmlAgilityPack;
using System.Text;

namespace landerist_library.Tools
{
    public class HtmlToText
    {

        private static readonly List<string> ListTextToRemoveNodeContains = new()
        {
            "cookie",  
            "javascript", 
            "navegador", 
            "browser", 
            "privacidad",
            "redes sociales",
            "formulario",
            "contacta",
            "contáctanos",
            "política",
            "rectificación",
            "chrome",
            "firefox",
            "safari",
            "explorer",           
            "robot",
            "error"
        };

        private static readonly List<string> ListTextToRemoveNodeEquals = new()
        {
            "aceptar",
            "enviar",
            "contactar"
        };


        private static readonly string selectContainsText = InitSelectContainsText();

        private static readonly string selectEqualsText = InitSelectEqualsText();


        private const string TagsToRemove =
            "//script | //nav | //footer | //style | //head | " +
            "//a | //code | //canvas | //input | //meta | //option | " +
            "//select | //progress | //svg | //textarea | //del | //aside " +
            "//button | // form" /*//form//form "*/; 

        

        private static string InitSelectContainsText()
        {
            var queryBuilder = new StringBuilder("//*[text()[");
            foreach (var word in ListTextToRemoveNodeContains)
            {
                queryBuilder.Append($"contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{word.ToLower()}') or ");
            }
            queryBuilder.Length -= 4;
            queryBuilder.Append("]]");

            return queryBuilder.ToString();
        }

        private static string InitSelectEqualsText()
        {
            return "//*[" + string.Join(" or ", ListTextToRemoveNodeEquals.Select(t => $"translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{t.Trim()}'")) + "]";
        }

        public static string GetText(HtmlDocument htmlDocument)
        {
            string text = string.Empty;
            try
            {
                RemoveNodes(htmlDocument, TagsToRemove);
                RemoveNodes(htmlDocument, selectContainsText);
                RemoveNodes(htmlDocument, selectEqualsText);
                var visibleText = GetVisibleText(htmlDocument);
                text = CleanText(visibleText);
            }
            catch { }
            return text;
        }

        private static void RemoveNodes(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = htmlNodeCollection.ToList();
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
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
