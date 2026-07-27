using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public static class PageBatchOrderer
{
    public static List<Page> SpreadByHost(IReadOnlyList<Page> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);

        Dictionary<string, Queue<Page>> pagesByHost = [];
        List<string> hosts = [];

        foreach (var page in pages)
        {
            var host = page.Host;
            if (!pagesByHost.TryGetValue(host, out var hostPages))
            {
                hostPages = [];
                pagesByHost[host] = hostPages;
                hosts.Add(host);
            }

            hostPages.Enqueue(page);
        }

        List<Page> spreadPages = new(pages.Count);
        while (spreadPages.Count < pages.Count)
        {
            foreach (var host in hosts)
            {
                var hostPages = pagesByHost[host];
                if (hostPages.Count > 0)
                {
                    spreadPages.Add(hostPages.Dequeue());
                }
            }
        }

        return spreadPages;
    }
}
