using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteNetworkService
{
    bool RefreshMainUri(Website website);

    bool RefreshRobotsTxt(Website website);

    bool RefreshIpAddress(Website website);
}
