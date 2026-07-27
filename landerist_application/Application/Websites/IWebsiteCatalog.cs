using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteCatalog
{
    IReadOnlyList<Website> GetAll();
    IReadOnlySet<string> GetHosts();
    Website Get(string host);
    bool Exists(string host);
    IReadOnlySet<string> GetUrls();
    IReadOnlyList<Website> GetWithSuccessfulStatus();
    IReadOnlyList<Website> GetWithUnsuccessfulStatus();
    IReadOnlyList<Website> GetWithoutStatus();
    IReadOnlyList<Website> GetNeedingRobotsTxtUpdate(DateTime updatedBefore);
    IReadOnlyList<Website> GetNeedingSitemapUpdate(DateTime updatedBefore);
    IReadOnlyList<Website> GetNeedingIpAddressUpdate(DateTime updatedBefore);
}
