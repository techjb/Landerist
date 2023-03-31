namespace landerist_library.Websites
{
    public class PageExtractor
    {
        private readonly Page Page;

        private readonly List<Page> Pages = new();
        public PageExtractor(Page page)
        {
            Page = page;
        }

        public List<Page> GetPages()
        {
            if (Page.HtmlDocument != null)
            {
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
            }

            return Pages;
        }

        private void GetPages(List<string?> links)
        {
            links = links.Distinct().ToList();
            foreach (var link in links)
            {
                if (!Uri.TryCreate(Page.Uri, link, out Uri? uri))
                {
                    continue;
                }
                if (!uri.Host.Equals(Page.Host) || uri.Equals(Page.Uri))
                {
                    continue;
                }
                if (Page.Website != null && !Page.Website.IsPathAllowed(uri))
                {
                    continue;
                }
                Page page = new(uri);
                Pages.Add(page);
            }
        }
    }
}
