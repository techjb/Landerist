using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class WebsiteAdministrationService
{
    public const string WEBSITES = "[WEBSITES]";

    private readonly WebsiteRefreshOperations RefreshOperations;
    private readonly WebsiteAdministrationReporting Reporting;
    private readonly WebsiteFileCleanup FileCleanup;

    public HashSet<Website> GetAll() => [.. Catalog.GetAll()];
    public HashSet<string> GetHosts() => [.. Catalog.GetHosts()];

    public Dictionary<string, Website> GetDicionaryStatusCodeOk()
    {
        Dictionary<string, Website> dictionary = new(StringComparer.OrdinalIgnoreCase);
        foreach (Website website in GetStatusCodeOk())
        {
            dictionary[website.Host] = website;
        }
        return dictionary;
    }

    public HashSet<Website> GetStatusCodeOk() => [.. Catalog.GetWithSuccessfulStatus()];
    public HashSet<Website> GetStatusCodeNotOk() => [.. Catalog.GetWithUnsuccessfulStatus()];
    public HashSet<Website> GetStatusCodeNull() => [.. Catalog.GetWithoutStatus()];

    public Website GetWebsite(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        return Catalog.Get(page.Host);
    }

    public Website GetWebsite(string host) => Catalog.Get(host);
    public bool Exists(string host) => Catalog.Exists(host);
    public IReadOnlyCollection<string> GetUrls() => [.. Catalog.GetUrls()];

    public void SetHttpStatusCodesToAll() => RefreshOperations.RefreshAllMainUris();
    public void SetHttpStatusCodesToNull() => RefreshOperations.RefreshMissingMainUris();
    public void SetRobotsTxt() => RefreshOperations.RefreshAllRobotsTxt();
    public void SetIpAdress() => RefreshOperations.RefreshAllIpAddresses();
    public void UpdateRobotsTxt() => RefreshOperations.RefreshOutdatedRobotsTxt();
    public void UpdateSitemaps() => RefreshOperations.RefreshOutdatedSitemaps();
    public void UpdateIpAddress() => RefreshOperations.RefreshOutdatedIpAddresses();

    public void CountCanAccesToMainUri() => Reporting.CountMainUriAccess();
    public void CountRobotsSiteMaps() => Reporting.CountSitemaps();
    public void InsertMainPages() => Reporting.InsertMainPages();

    public void Delete(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        Delete(GetWebsite(uri.Host));
    }

    public void Delete(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        DeleteWithRelations(website);
    }

    public void DeleteAll()
    {
        Maintenance.DeleteAll();
        PageMaintenance.DeleteAll();
        DeleteAllListings();
    }

    public void DeleteAllListings() => ListingMaintenance.DeleteAll();
    public void DeleteFromFile() => FileCleanup.DeleteWebsitesWithoutListingUrl();
}
