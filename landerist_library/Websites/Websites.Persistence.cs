using landerist_library.Application.Persistence;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;

namespace landerist_library.Websites;

public partial class Websites
{
    private static readonly WebsitePersistenceService Persistence = new(new WebsiteRepository(new DataBase()));

    public static bool Insert(Website website) => Persistence.Insert(website);

    public static bool Update(Website website) => Persistence.Update(website);

    private static bool DeleteRecord(Website website) => Persistence.Delete(website);
}