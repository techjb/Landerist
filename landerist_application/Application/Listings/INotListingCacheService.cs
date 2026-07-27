using landerist_library.Pages;

namespace landerist_library.Application.Listings;

public interface INotListingCacheService
{
    bool Insert(Page page);

    bool Contains(Page page);
}
