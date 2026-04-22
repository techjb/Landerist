using HtmlAgilityPack;
using landerist_library.Database;

namespace landerist_library.Tools
{
    public class HtmlToText
    {
        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

        private static readonly HashSet<string> TagsToRemove =
        [
            "//head",
            "//script",
            "//style",
            //"//header",
            "//nav",
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
            "//button",
            "//form[not(.//input[@id='__VIEWSTATE' or @id='__VIEWSTATEGENERATOR' or @id='__EVENTVALIDATION'])]",
            "//input",
            "//*[contains(@style, 'text-decoration: line-through')]",
            "//*[contains(@style, 'text-decoration:line-through')]"
        ];

        private static readonly HashSet<string> TagsToClearClassAndId =
        [
            "//html",
            "//body",
            "//main",
        ];

        private static readonly HashSet<string> IdOrClassContains =
        [
            "similar",
            "results",
            "resultados",
            "related products",
            "busca",
            "search",
            "suscrib",
            "privac",
            "cookie",
            "danger",
            "hipoteca",
            "filtro",
            "filter",
            "news",
            "noticias",
            "modal-dialog",
            "poi-container",
            "-slider",
            "related"
        ];

        private static readonly HashSet<string> TextContains =
        [
            " cookie",
            " javascript",
            " navegador",
            " navigation",
            " browser",
            " política de privacidad",
            " redes sociales",
            " formulario",
            " política",
            " rectificación",
            "chrome",
            "firefox",
            "safari",
            "explorer",
            " robot",
            " error",
            " analytics",
            " ajustes",
            " activar",
            " imprimir",
            " recomendar",
            " similares",
            " enviado",
            " hipoteca",
            " buscador",
            " legal",
            "sesión",
            " spam",            
            "comparte este inmueble",
            "suscríbete",
            "loading",
            "cargando",
            "derechos reservados",
            "contraseña",
            "password",
            "registrarse",
            "full screen"
        ];

        private static readonly HashSet<string> TextEquals =
        [
            "aceptar",
            "enviar",
            "enviar por e-mail",
            "enviar por email",
            "validar",
            "contactar",
            "contact",
            "contáctanos",
            "deja tu respuesta",
            "compartir",
            "compartir esto",
            "es",
            "en",
            "de",
            "fr",
            "favorito",
            "favorit",
            "descartar",
            "imprimir",
            "print",
            "cerrar",
            "close",
            "filtros",
            "filters",
            "buscar",
            "search",
            "financiación",
            "legal",
            "necesarias",
            "necessary",
            "siempre activado",
            "botón",
            "síganos",
            "linkedin",
            "menú",
            "facebook",
            "twitter",
            "idioma",
            "fotos",
            "fotografías",
            "imagenes",
            "mapa",
            "plano",
            "vídeo",
            "vídeos",
            "recomendar",
            "portada",
            "login",
            "financiación",
            "galería",
            "registro",
            "admin",
            "favoritos",
            "leer más",
            "ver más",
            "read more",
            "posted by",
            "publicado por",
            "analytics"
        ];

        private static readonly HashSet<string> RecommendationSectionHints =
        [
            "anuncios similares",
            "anuncios relacionados",
            "inmuebles similares",
            "inmuebles relacionados",
            "propiedades similares",
            "propiedades relacionadas",
            "pisos similares",
            "casas similares",
            "otros inmuebles",
            "otros anuncios",
            "también te puede interesar",
            "tambien te puede interesar",
            "te puede interesar",
            "similar properties",
            "similar homes",
            "related properties",
            "related listings",
            "you may also like",
            "recommended",
            "featured properties"
        ];

        private static readonly HashSet<string> RecommendationContainerTags =
        [
            "section",
            "div",
            "aside",
            "ul",
            "ol",
            "nav"
        ];

        private static readonly HashSet<string> HeadingTags =
        [
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header"
        ];

        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static readonly string XpathTagsToClearClassAndId = InitXpathTagsToClearClassAndId();

        private static readonly string XpathIdContains = InitXpathIdContains();

        private static readonly string XpathClassContains = InitXpathClassContains();

        private static readonly string XpathTextContains = InitXpathTextContains();

        private static readonly string XpathTextEquals = InitXpathTextEquals();

        private const int MinimumRecommendationLinks = 3;

        private const int MinimumAggressiveRecommendationLinks = 8;

        private static string InitXpathTagsToRemove()
        {
            return string.Join(" | ", TagsToRemove.ToList());
        }

        private static string InitXpathTagsToClearClassAndId()
        {
            return string.Join(" | ", TagsToClearClassAndId.ToList());
        }

        private static string InitXpathIdContains()
        {
            return ToXpathContains(IdOrClassContains, "@id");
        }

        private static string InitXpathClassContains()
        {
            return ToXpathContains(IdOrClassContains, "@class");
        }

        private static string InitXpathTextContains()
        {
            var enumerable = TextContains.Select(word =>
                $"contains(translate(., '{UppercaseLetters}', '{LowercaseLetters}'), '{word.ToLowerInvariant()}')");

            return "//text()[" + string.Join(" or ", enumerable) + "]";
        }

        private static string InitXpathTextEquals()
        {
            return "//*[" + string.Join(" or ", TextEquals.Select(word => $"translate(normalize-space(text()), '{UppercaseLetters}', '{LowercaseLetters}')='{word.Trim()}'")) + "]";
        }

        private static string ToXpathContains(HashSet<string> list, string selector)
        {
            var enumerable = list.Select(word => $"contains(translate({selector}, '{UppercaseLetters}', '{LowercaseLetters}'), '{word.ToLowerInvariant()}')");
            return "//*[" + string.Join(" or ", enumerable) + "]";
        }

        public static string GetText(HtmlDocument htmlDocument)
        {
            string text = string.Empty;
            try
            {
                RemoveNodes(htmlDocument, XpathTagsToRemove);
                ClearClassAndId(htmlDocument, XpathTagsToClearClassAndId);
                RemoveNodes(htmlDocument, XpathIdContains);
                RemoveNodes(htmlDocument, XpathClassContains);
                RemoveNodes(htmlDocument, XpathTextContains);
                RemoveNodesEquals(htmlDocument, XpathTextEquals);
                RemoveLikelyRecommendationSections(htmlDocument);

                var visibleText = GetVisibleText(htmlDocument);
                text = CleanText(visibleText);
            }
            catch { }
            return text;
        }

        private static void ClearClassAndId(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    ClearClassAndId(node, "class");
                    ClearClassAndId(node, "id");
                }
            }
        }

        private static void ClearClassAndId(HtmlNode htmlNode, string attributeName)
        {
            if (htmlNode.Attributes.Any(att => att.Name == attributeName))
            {
                htmlNode.Attributes[attributeName].Value = string.Empty;
            }
        }

        private static void RemoveNodes(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    node.Remove();
                }
            }
        }

        private static void RemoveNodesEquals(HtmlDocument htmlDocument, string select)
        {
            var htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes(select);
            if (htmlNodeCollection != null)
            {
                List<HtmlNode> nodesToRemove = [.. htmlNodeCollection];
                foreach (var node in nodesToRemove)
                {
                    var text = node.InnerText.Trim();
                    if (TextEquals.Contains(text, StringComparer.OrdinalIgnoreCase))
                    {
                        node.Remove();
                    }
                }
            }
        }

        private static void RemoveLikelyRecommendationSections(HtmlDocument htmlDocument)
        {
            var candidates = htmlDocument.DocumentNode.Descendants()
                .Where(node => node.NodeType == HtmlNodeType.Element)
                .Where(node => RecommendationContainerTags.Contains(node.Name))
                .OrderBy(node => node.Ancestors().Count())
                .ToList();

            HashSet<HtmlNode> nodesToRemove = [];
            foreach (var node in candidates)
            {
                if (node.ParentNode == null || HasAncestorMarkedToRemove(node, nodesToRemove))
                {
                    continue;
                }

                if (IsLikelyRecommendationSection(node))
                {
                    nodesToRemove.Add(node);
                }
            }

            foreach (var node in nodesToRemove)
            {
                node.Remove();
            }
        }

        private static bool HasAncestorMarkedToRemove(HtmlNode node, HashSet<HtmlNode> nodesToRemove)
        {
            return node.Ancestors().Any(nodesToRemove.Contains);
        }

        private static bool IsLikelyRecommendationSection(HtmlNode node)
        {
            string text = NormalizeText(node.InnerText);
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            int wordCount = Strings.CountWords(text);
            if (wordCount == 0)
            {
                return false;
            }

            var anchors = node.Descendants("a")
                .Select(anchor => NormalizeText(anchor.InnerText))
                .Where(anchorText => !string.IsNullOrEmpty(anchorText))
                .ToList();

            int linkCount = anchors.Count;
            if (linkCount == 0)
            {
                return false;
            }

            bool hasHint = HasRecommendationHint(node, text);
            bool hasRepeatedLinkedChildren = HasRepeatedLinkedChildren(node);
            int linkWords = anchors.Sum(Strings.CountWords);
            double linkDensity = (double)linkWords / Math.Max(1, wordCount);

            if (hasHint && (linkCount >= MinimumRecommendationLinks || hasRepeatedLinkedChildren || linkDensity >= 0.20d))
            {
                return true;
            }

            return linkCount >= MinimumAggressiveRecommendationLinks &&
                hasRepeatedLinkedChildren &&
                linkDensity >= 0.55d &&
                wordCount <= 350;
        }

        private static bool HasRecommendationHint(HtmlNode node, string normalizedText)
        {
            string leadingText = TakeWords(normalizedText, 60);
            if (RecommendationSectionHints.Any(leadingText.Contains))
            {
                return true;
            }

            string headingText = NormalizeText(string.Join(" ",
                node.Descendants()
                    .Where(child => HeadingTags.Contains(child.Name))
                    .Take(3)
                    .Select(child => child.InnerText)));

            if (!string.IsNullOrEmpty(headingText) && RecommendationSectionHints.Any(headingText.Contains))
            {
                return true;
            }

            string attributeText = NormalizeText(
                node.GetAttributeValue("id", string.Empty) + " " +
                node.GetAttributeValue("class", string.Empty));

            return attributeText.Contains("related", StringComparison.Ordinal) ||
                attributeText.Contains("recommend", StringComparison.Ordinal) ||
                attributeText.Contains("featured", StringComparison.Ordinal) ||
                attributeText.Contains("destacad", StringComparison.Ordinal) ||
                attributeText.Contains("nearby", StringComparison.Ordinal);
        }

        private static bool HasRepeatedLinkedChildren(HtmlNode node)
        {
            var linkedChildren = node.ChildNodes
                .Where(child => child.NodeType == HtmlNodeType.Element)
                .Where(child => child.Descendants("a").Any(anchor => !string.IsNullOrWhiteSpace(anchor.InnerText)))
                .ToList();

            if (linkedChildren.Count < 3)
            {
                return false;
            }

            int repeatedStructureCount = linkedChildren
                .Select(GetStructureSignature)
                .GroupBy(signature => signature)
                .Select(group => group.Count())
                .DefaultIfEmpty(0)
                .Max();

            return repeatedStructureCount >= 3 || linkedChildren.Count >= 4;
        }

        private static string GetStructureSignature(HtmlNode node)
        {
            string children = string.Join(",",
                node.ChildNodes
                    .Where(child => child.NodeType == HtmlNodeType.Element)
                    .Select(child => child.Name)
                    .Take(4));

            return $"{node.Name}>{children}";
        }

        private static string NormalizeText(string? text)
        {
            return Strings.Clean(HtmlEntity.DeEntitize(text ?? string.Empty)).ToLowerInvariant();
        }

        private static string TakeWords(string text, int maxWords)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            string[] words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (words.Length <= maxWords)
            {
                return text;
            }

            return string.Join(" ", words.Take(maxWords));
        }

        private static IEnumerable<string>? GetVisibleText(HtmlDocument htmlDocument)
        {
            var visibleNodes = htmlDocument.DocumentNode.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text)
                .Where(n => !string.IsNullOrWhiteSpace(n.InnerHtml));

            return visibleNodes.Select(n => n.InnerHtml.Trim());
        }

        private static string CleanText(IEnumerable<string>? lines)
        {
            List<string> cleanedLines = [];
            if (lines == null)
            {
                return string.Empty;
            }

            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string? decodedLine = HtmlEntity.DeEntitize(line)?.Trim();
                if (string.IsNullOrEmpty(decodedLine))
                {
                    continue;
                }

                if (IsSymbol(decodedLine))
                {
                    continue;
                }

                if (TextEquals.Contains(decodedLine, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                cleanedLines.Add(decodedLine);

                if (Configuration.Config.WORDS_ENABLED)
                {
                    InsertWords(decodedLine);
                }
            }

            string text = string.Join(" ", cleanedLines);
            return Strings.Clean(text);
        }

        public static bool IsSymbol(string input)
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

        public static void InsertWords(string text)
        {
            string cleaned = Strings.Clean(text);
            if (string.IsNullOrEmpty(cleaned))
            {
                return;
            }

            if (Strings.IsNumeric(text))
            {
                return;
            }

            if (Strings.CountWords(cleaned) <= 3)
            {
                Words.Insert(cleaned);
            }
        }
    }
}
