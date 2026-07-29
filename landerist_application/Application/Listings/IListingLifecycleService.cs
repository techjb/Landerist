using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Listings;

public interface IListingLifecycleService
{
    void Apply(Page page, Listing? listing);

    Task ApplyAsync(
        Page page,
        Listing? listing,
        CancellationToken cancellationToken = default);
}
