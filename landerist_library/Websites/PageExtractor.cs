using System.Globalization;
using System.Linq;

namespace landerist_library.Websites
{
    public class PageExtractor
    {
        private readonly Page Page;

        private readonly SortedSet<Page> Pages = new();
        public PageExtractor(Page page)
        {
            Page = page;
        }

        public SortedSet<Page> GetPages()
        {
            if(Page.HtmlDocument == null)
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
                if(page != null && !Pages.Contains(page))
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
    }
}
