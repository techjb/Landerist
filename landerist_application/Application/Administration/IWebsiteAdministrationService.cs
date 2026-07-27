using landerist_library.Websites;

namespace landerist_library.Application.Administration;

public interface IWebsiteAdministrationService
{
    IReadOnlyCollection<string> GetUrls();
    bool Exists(string host);
    bool Insert(Website website);
    bool Update(Website website);
    bool DeleteWithRelations(Website website);
    bool InsertMainPage(Website website);
}
