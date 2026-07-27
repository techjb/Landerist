using landerist_library.Websites;

namespace landerist_library.Application.Persistence;

public interface IWebsitePersistenceService
{
    bool Insert(Website website);

    bool Update(Website website);

    bool Delete(Website website);
}
