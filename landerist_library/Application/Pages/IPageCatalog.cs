using landerist_library.Pages;
using landerist_library.Websites;

namespace landerist_library.Application.Pages;

public interface IPageCatalog
{
    Page? GetByHash(string uriHash);

    IReadOnlyList<Page> GetByWebsite(Website website);
}
