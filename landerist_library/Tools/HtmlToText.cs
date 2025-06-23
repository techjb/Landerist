using HtmlAgilityPack;
using landerist_library.Database;

namespace landerist_library.Tools
{
    public class HtmlToText
    {
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
            //"propiedades",
            //"properties",
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
            "-slider"
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
            //"escríbenos",
            //"contacta",
            //"contáctanos",
            //"no encontrada",
            //"no existe",
            //"404 Not Found",
            //"algo salió mal",
            //"no pudimos encontrar",
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


        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static readonly string XpathTagsToClearClassAndId = InitXpathTagsToClearClassAndId();

        private static readonly string XpathIdContains = InitXpathIdContains();

        private static readonly string XpathClassContains = InitXpathClassContains();

        private static readonly string XpathTextContains = InitXpathTextContains();

        private static readonly string XpathTextEquals = InitXpathTextEquals();

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
            string xpath = ToXpathContains(TextContains, ".");
            return xpath.Replace("//*", "//*[text()") + "]";
        }

        private static string InitXpathTextEquals()
        {
            return "//*[" + string.Join(" or ", TextEquals.Select(word => $"translate(normalize-space(text()), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{word.Trim()}'")) + "]";
        }

        private static string ToXpathContains(HashSet<string> list, string selector)
        {
            var enumerable = list.Select(word => $"contains(translate({selector}, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{word.ToLower()}')");
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
                    var text = node.InnerText.ToLower();
                    if (TextEquals.Contains(text))
                    {
                        node.Remove();
                    }
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
