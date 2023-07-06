﻿using HtmlAgilityPack;
using RTools_NTS.Util;
using System.Text;
using static Google.Cloud.Language.V1.PartOfSpeech.Types;

namespace landerist_library.Tools
{
    public class HtmlToText
    {

        private static readonly List<string> TextContains = new()
        {
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
            "no encontrada",
            "no existe",
            "404 Not Found",
            "algo salió mal",
            "no pudimos encontrar",
            "comparte este inmueble",
            "suscríbete",
            "loading"

        };

        private static readonly List<string> ClassAndIdContains = new()
        {
            "similar",
            //"result",            
            "busca",            
            "search",
            "suscrib",
            "privac",
            "cookie",
            "danger",
            "hipoteca",
            "filtro",
            "filter"
        };

        private static readonly List<string> TextEquals = new()
        {
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
            "financiación"
        };

        private static readonly List<string> TagsToRemove = new()
        {
            "//script",
            "//nav",
            "//footer",
            "//style",
            "//head",
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
            "//aside",
            "//button",
            "//form[not(.//input[@id='__VIEWSTATE' or @id='__VIEWSTATEGENERATOR' or @id='__EVENTVALIDATION'])]",
            "//input",
            "//*[contains(@style, 'text-decoration: line-through')]",
            "//*[contains(@style, 'text-decoration:line-through')]"
        };

        private static readonly string XpathTextContains = InitXpathTextContains();

        private static readonly string XpathIdContains = InitXpathIdContains();

        private static readonly string XpathClassContains = InitXpathClassContains();

        private static readonly string XpathTextEquals = InitXpathTextEquals();

        private static readonly string XpathTagsToRemove = InitXpathTagsToRemove();

        private static string InitXpathTextContains()
        {
            string xpath = ToXpathContains(TextContains, ".");
            return xpath.Replace("//*", "//*[text()") + "]";
        }

        private static string InitXpathIdContains()
        {
            return ToXpathContains(ClassAndIdContains, "@id");
        }

        private static string InitXpathClassContains()
        {
            return ToXpathContains(ClassAndIdContains, "@class");
        }

        private static string InitXpathTextEquals()
        {
            return "//*[" + string.Join(" or ", TextEquals.Select(word => $"translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')='{word.Trim()}'")) + "]";
        }

        private static string ToXpathContains(List<string> list, string selector)
        {
            var enumerable = list.Select(word => $"contains(translate({selector}, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), '{word.ToLower()}')");
            return "//*[" + string.Join(" or ", enumerable) + "]";
        }

        private static string InitXpathTagsToRemove()
        {
            return string.Join(" | ", TagsToRemove.ToList());
        }

        public static string GetText(HtmlDocument htmlDocument)
        {
            string text = string.Empty;
            try
            {

                RemoveNodes(htmlDocument, XpathTagsToRemove);
                RemoveNodes(htmlDocument, XpathIdContains);
                RemoveNodes(htmlDocument, XpathClassContains);
                RemoveNodes(htmlDocument, XpathTextContains);
                RemoveNodes(htmlDocument, XpathTextEquals);

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
