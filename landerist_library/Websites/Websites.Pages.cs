using landerist_library.Configuration;
using landerist_library.Infrastructure.Sql;
using landerist_library.Pages;

namespace landerist_library.Websites;

public partial class Websites
{
    private static readonly WebsitePageMetricsRepository PageMetrics =
        new(global::landerist_library.Database.LegacyDatabase.Create());

    public static bool InsertMainPage(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return Pages.Pages.Insert(new Page(website));
    }


    public static List<Page> GetUnknownPageType(Website website) => Pages.Pages.GetUnknowPageType(website);

    public static List<Page> GetNonScrapedPages(Website website) => Pages.Pages.GetNonScrapedPages(website);

    public static int GetNumPages(Website website) => PageMetrics.CountPages(website.Host);

    public static bool AchievedMaxNumberOfPages(Website website) =>
        GetNumPages(website) >= Config.MAX_PAGES_PER_WEBSITE;
}