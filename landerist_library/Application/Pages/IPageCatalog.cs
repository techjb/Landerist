using landerist_library.Pages;

namespace landerist_library.Application.Pages;

public interface IPageCatalog
{
    Page? GetByHash(string uriHash);
}