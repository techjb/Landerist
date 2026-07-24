using landerist_library.Websites;

namespace landerist_library.Application.Websites;

public interface IWebsiteMaintenanceService
{
    bool DeleteAll();
}

public interface IWebsiteMetricsService
{
    int CountPages(Website website);
    bool HasAchievedMaximumPages(Website website);
}

