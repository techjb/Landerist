using landerist_library.Application;
using landerist_library.Application.Persistence;

namespace landerist_library.Websites;

public partial class Websites
{
    private static IWebsitePersistenceService Persistence =>
        LanderistApplication.Services.WebsitePersistence;

    public static bool Insert(Website website) => Persistence.Insert(website);

    public static bool Update(Website website) => Persistence.Update(website);

    private static bool DeleteRecord(Website website) => Persistence.Delete(website);
}