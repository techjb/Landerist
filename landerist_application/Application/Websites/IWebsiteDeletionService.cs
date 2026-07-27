using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteDeletionService
{
    bool DeleteWithRelations(Website website);
}
