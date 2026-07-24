using landerist_library.Pages;

namespace landerist_library.Websites;

public partial class Websites
{
    public static bool InsertMainPage(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return Pages.Pages.Insert(new Page(website));
    }

    public static List<Page> GetUnknownPageType(Website website) =>
        [.. PageQueries.GetUnknown(website)];

    public static List<Page> GetNonScrapedPages(Website website) =>
        [.. PageQueries.GetNonScraped(website)];

    public static int GetNumPages(Website website) =>
        Metrics.CountPages(website);

    public static bool AchievedMaxNumberOfPages(Website website) =>
        Metrics.HasAchievedMaximumPages(website);
}
