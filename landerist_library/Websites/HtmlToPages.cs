using HtmlAgilityPack;

namespace landerist_library.Websites
{
    public class HtmlToPages
    {
        private readonly HtmlDocument HtmlDocument;

        private readonly Website Website;

        private readonly Uri Uri;
        public HtmlToPages(HtmlDocument htmlDocument, Website website, Uri uri)
        {
            HtmlDocument = htmlDocument;
            Website = website;
            Uri = uri;
        }

        public List<Page> GetPages()
        {
            try
            {
                var links = HtmlDocument.DocumentNode.Descendants("a")
                   .Where(a => !a.Attributes["rel"]?.Value.Contains("nofollow") ?? true)
                   .Select(a => a.Attributes["href"]?.Value)
                   .Where(href => !string.IsNullOrWhiteSpace(href))
                   .ToList();

                if (links != null)
                {
                    return GetPages(links);
                }
            }
            catch { }
            
            return new List<Page>();
        }

        private List<Page> GetPages(List<string?> links)
        {
            links = links.Distinct().ToList();
            List<Page> pages = new();
            foreach (var link in links)
            {
                if (!Uri.TryCreate(Uri, link, out Uri? uri))
                {
                    continue;
                }
                if (!uri.Host.Equals(Uri.Host) || uri.Equals(Uri))
                {
                    continue;
                }
                if (Website != null && !Website.IsPathAllowed(uri))
                {
                    continue;
                }
                Page page = new(uri);
                pages.Add(page);
            }
            return pages;
        }

    }
}
