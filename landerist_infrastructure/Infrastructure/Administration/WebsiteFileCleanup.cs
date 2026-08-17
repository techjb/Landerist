using landerist_library.Application.Administration;
using landerist_library.Application.Websites;
using landerist_library.Configuration;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Infrastructure.Administration;

internal sealed class WebsiteFileCleanup(
    IWebsiteCatalog catalog,
    IWebsiteMetricsService metrics,
    IWebsiteDeletionService deletion)
{
    internal void DeleteWebsitesWithoutListingUrl()
    {
        string file = AppConfig.INSERT_DIRECTORY + "HostMainUri.csv";
        DataTable dataTable = Tools.Csv.ToDataTable(file);
        HashSet<string> hosts = [];
        foreach (DataRow row in dataTable.Rows)
        {
            string host = (string)row[0];
            string listingUrl = ((string)row[2]).Trim();
            if (listingUrl.Length == 0) hosts.Add(host);
        }

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
