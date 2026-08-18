using System.Data;

namespace landerist_library.Infrastructure.Administration;

internal interface IWebsiteCleanupFileReader
{
    IReadOnlyCollection<string> ReadHostsWithoutListingUrl(string filePath);
}

internal sealed class WebsiteCleanupCsvReader : IWebsiteCleanupFileReader
{
    public IReadOnlyCollection<string> ReadHostsWithoutListingUrl(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        DataTable dataTable = Tools.Csv.ToDataTable(filePath);
        HashSet<string> hosts = [];
        foreach (DataRow row in dataTable.Rows)
        {
            string host = (string)row[0];
            string listingUrl = ((string)row[2]).Trim();
            if (listingUrl.Length == 0)
            {
                hosts.Add(host);
            }
        }
        return hosts;
    }
}
