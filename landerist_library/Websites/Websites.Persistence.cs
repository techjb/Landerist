using landerist_library.Application;
using landerist_library.Application.Persistence;
using landerist_library.Application.Websites;

namespace landerist_library.Websites;

public partial class Websites
{
    private static IWebsitePersistenceService Persistence =>
        LanderistApplication.Services.WebsitePersistence;

    private static IWebsiteDeletionService Deletion =>
        LanderistApplication.Services.WebsiteDeletion;

    public static bool Insert(Website website) => Persistence.Insert(website);

    public static bool Update(Website website) => Persistence.Update(website);

    public static bool DeleteWithRelations(Website website) =>
        Deletion.DeleteWithRelations(website);
}
