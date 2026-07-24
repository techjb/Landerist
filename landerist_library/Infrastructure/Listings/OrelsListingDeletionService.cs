using landerist_library.Application.Listings;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Listings;

public sealed class OrelsListingDeletionService : IListingDeletionService
{
    private readonly IListingMaintenanceService _maintenance;

    public OrelsListingDeletionService(IListingMaintenanceService maintenance)
    {
        ArgumentNullException.ThrowIfNull(maintenance);
        _maintenance = maintenance;
    }

    public void Delete(Page page)
    {
        ArgumentNullException.ThrowIfNull(page);
        _maintenance.Delete(page.UriHash);
    }
}
