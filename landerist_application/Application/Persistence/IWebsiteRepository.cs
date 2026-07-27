using landerist_library.Websites;

namespace landerist_library.Application.Persistence;

public interface IWebsiteRepository
{
    bool Insert(Website website);
    bool Update(Website website);
    bool Delete(string host);
}