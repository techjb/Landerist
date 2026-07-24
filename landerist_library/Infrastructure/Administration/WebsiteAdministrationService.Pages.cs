using landerist_library.Websites;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Administration;

public sealed partial class WebsiteAdministrationService
{
    public bool InsertMainPage(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return PagePersistence.Insert(new Page(website));
    }

    public List<Page> GetUnknownPageType(Website website) =>
        [.. PageQueries.GetUnknown(website)];

    public List<Page> GetNonScrapedPages(Website website) =>
        [.. PageQueries.GetNonScraped(website)];

    public int GetNumPages(Website website) =>
        Metrics.CountPages(website);

    public bool AchievedMaxNumberOfPages(Website website) =>
        Metrics.HasAchievedMaximumPages(website);
}

