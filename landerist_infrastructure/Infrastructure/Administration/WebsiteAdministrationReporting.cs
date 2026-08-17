using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;
using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Infrastructure.Administration;

internal sealed class WebsiteAdministrationReporting(
    IWebsiteCatalog catalog,
    IWebsiteRobotsPolicy robotsPolicy,
    IPagePersistenceService pagePersistence)
{
    internal void CountMainUriAccess()
    {
        int allowed = 0;
        int denied = 0;
        foreach (Website website in catalog.GetWithSuccessfulStatus())
        {
            if (robotsPolicy.IsAllowed(website, website.MainUri)) allowed++;
            else denied++;
            Console.WriteLine($"Yes: {allowed} No: {denied} {GetDisplayText(website)}");
        }
    }

    internal void CountSitemaps()
    {
        int count = 0;
        foreach (Website website in catalog.GetWithSuccessfulStatus())
        {
            count += robotsPolicy.GetSitemapUrls(website).Count;
            Console.WriteLine($"SiteMaps: {count}");
        }
    }

    internal void InsertMainPages()
    {
        IReadOnlyCollection<Website> websites = [.. catalog.GetWithSuccessfulStatus()];
        int inserted = 0;
        int errors = 0;
        foreach (Website website in websites)
        {
            if (pagePersistence.Insert(new Page(website))) inserted++;
            else errors++;
            Console.WriteLine($"Inserted: {inserted} Errors: {errors} From: {websites.Count}");
        }
    }

    private static string GetDisplayText(Website website) => website.MainUri?.ToString() ?? website.Host;
}
