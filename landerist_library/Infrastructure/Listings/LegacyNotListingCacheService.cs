using landerist_library.Application.Listings;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Listings;

public sealed class LegacyNotListingCacheService : INotListingCacheService
{
    public bool Insert(Page page) =>
        global::landerist_library.Pages.Pages.InsertToNotListingCache(page);
}
