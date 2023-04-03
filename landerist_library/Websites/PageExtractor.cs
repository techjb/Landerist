using System.IO;

namespace landerist_library.Websites
{
    public class PageExtractor
    {
        private readonly Page Page;

        private readonly SortedSet<Page> Pages = new();

        private readonly HashSet<string> ES_ProhibitedWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "contact",
            "privacidad",
            "legal",
            "filosofia",
            "nosotros",
            "conozcanos",
            "servicios",
            "acercade",
            "consejos",
            "quienessomos",
            "colaboradores",
            "equipo",
            "noticias",
            "tasacion",
            "empresa",
            "trabaja",
            "empleo",
            "localizacion",
            "dondeestamos",
            "valoracion",
            "valorar",
            "actualidad",
            "articulos",
            "blog",
            "vendetuinmueble",
            "condiciones",
            "historia",
            "franquicia",
            "empleo",
            "publicatuinmueble",
            "favorit",
        };

        private readonly HashSet<string> ES_ProhibitedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "en",
            "en-gb",
            "de",
            "de-de",            
            "fr",
            "fr-fr"
        };

        public PageExtractor(Page page)
        {
            Page = page;
        }

        public SortedSet<Page> GetPages()
        {
            if (Page.HtmlDocument == null)
            {
                return Pages;
            }
            try
            {
                var links = Page.HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrWhiteSpace(href))
                   .ToList();

                if (links != null)
                {
                    GetPages(links);
                }
            }
            catch { }

            return Pages;
        }

        private void GetPages(List<string?> links)
        {
            links = links.Distinct().ToList();
            foreach (var link in links)
            {
                var page = GetPage(link);
                if (page != null && !Pages.Contains(page))
                {
                    Pages.Add(page);
                }
            }
        }

        private Page? GetPage(string? link)
        {
            if (link == null)
            {
                return null;
            }
            if (!Uri.TryCreate(Page.Uri, link, out Uri? uri))
            {
                return null;
            }
            UriBuilder uriBuilder = new(uri)
            {
                Fragment = "",
            };
            uri = uriBuilder.Uri;
            if (ContainsProhibitedWord(uri))
            {
                return null;
            }
            if (ContainsProhibitedPath(uri))
            {
                return null;
            }
            if (!uri.Host.Equals(Page.Host) || uri.Equals(Page.Uri))
            {
                return null;
            }
            if (Page.Website != null && !Page.Website.IsPathAllowed(uri))
            {
                return null;
            }
            return new Page(uri);
        }

        private bool ContainsProhibitedWord(Uri uri)
        {
            string absolutePath = uri.AbsolutePath
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                ;
            return ES_ProhibitedWords.Contains(absolutePath);
        }

        private bool ContainsProhibitedPath(Uri uri)
        {
            string absolutePath = uri.AbsolutePath;
            string[] pathComponents = absolutePath.Substring(0, absolutePath.LastIndexOf('/')).Split('/');

            foreach (string path in pathComponents)
            {
                if (ES_ProhibitedPaths.Contains(path))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
