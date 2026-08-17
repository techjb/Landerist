using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Administration;

internal sealed class WebsiteRefreshOperations(
    IWebsiteCatalog catalog,
    IWebsitePersistenceService persistence,
    IWebsiteNetworkService network,
    IWebsiteSitemapService sitemaps)
{
    internal void RefreshAllMainUris() => RefreshWithProgress(catalog.GetAll(), network.RefreshMainUri);
    internal void RefreshMissingMainUris() => RefreshWithProgress(catalog.GetWithoutStatus(), network.RefreshMainUri);
    internal void RefreshAllRobotsTxt() => RefreshWithProgress(catalog.GetAll(), network.RefreshRobotsTxt);
    internal void RefreshAllIpAddresses() => RefreshWithProgress(catalog.GetAll(), network.RefreshIpAddress);

    internal void RefreshOutdatedRobotsTxt() => RefreshOutdated(
        [.. catalog.GetNeedingRobotsTxtUpdate(DateTime.Now.AddDays(-1))],
        "robots.txt",
        website => network.RefreshRobotsTxt(website));

    internal void RefreshOutdatedSitemaps() => RefreshOutdated(
        [.. catalog.GetNeedingSitemapUpdate(DateTime.Now.AddDays(-1))],
        "sitemaps",
        website => sitemaps.RefreshSitemap(website));

    internal void RefreshOutdatedIpAddresses() => RefreshOutdated(
        [.. catalog.GetNeedingIpAddressUpdate(DateTime.Now.AddDays(-1))],
        "ip address",
        website => network.RefreshIpAddress(website));

    private void RefreshWithProgress(IEnumerable<Website> source, Func<Website, bool> refresh)
    {
        IReadOnlyCollection<Website> websites = [.. source];
        int total = websites.Count;
        int processed = 0;
        int succeeded = 0;
        int errors = 0;

        Parallel.ForEach(websites, website =>
        {
            try
            {
                bool success = refresh(website);
                if (success)
                {
                    persistence.Update(website);
                }

                int current = Interlocked.Increment(ref processed);
                if (success) Interlocked.Increment(ref succeeded);
                else Interlocked.Increment(ref errors);

                double percentage = Math.Round((double)current * 100 / total, 2);
                Console.WriteLine($"{current}/{total} ({percentage}%) Success: {succeeded} Errors: {errors} {GetDisplayText(website)}");
            }
            finally
            {
                website.Dispose();
            }
        });
    }

    private void RefreshOutdated(IReadOnlyCollection<Website> websites, string resourceName, Action<Website> refresh)
    {
        if (websites.Count == 0) return;

        Console.WriteLine($"Updating {resourceName} of {websites.Count} websites");
        Parallel.ForEach(websites, website =>
        {
            try
            {
                refresh(website);
                persistence.Update(website);
            }
            finally
            {
                website.Dispose();
            }
        });
    }

    private static string GetDisplayText(Website website) => website.MainUri?.ToString() ?? website.Host;
}
