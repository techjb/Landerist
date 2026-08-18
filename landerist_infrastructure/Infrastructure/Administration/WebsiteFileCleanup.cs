using landerist_library.Application.Administration;
using landerist_library.Application.Websites;
using landerist_library.Websites;
using landerist_library.Infrastructure.Runtime;

namespace landerist_library.Infrastructure.Administration;

internal sealed class WebsiteFileCleanup(
    IWebsiteCatalog catalog,
    IWebsiteMetricsService metrics,
    IWebsiteDeletionService deletion,
    AdministrationOptions options,
    IWebsiteCleanupFileReader fileReader)
{
    internal void DeleteWebsitesWithoutListingUrl()
    {
        IReadOnlyCollection<string> hosts =
            fileReader.ReadHostsWithoutListingUrl(options.WebsiteCleanupFilePath);

        int total = hosts.Count;
        int processed = 0;
        Parallel.ForEach(hosts, host =>
        {
            Website website = catalog.Get(host);
            try
            {
                if (metrics.CountPages(website) > 0) deletion.DeleteWithRelations(website);
            }
            finally
            {
                website.Dispose();
            }

            int current = Interlocked.Increment(ref processed);
            Console.WriteLine($"{current}/{total}");
        });
    }
}
