using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteCatalog
{
    IReadOnlyList<Website> GetAll();

    IReadOnlySet<string> GetHosts();

    Website Get(string host);

    bool Exists(string host);
}